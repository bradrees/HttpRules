using System.Threading.Tasks;

namespace HttpRulesCore
{
    #region Using Directives

    using System.Collections.Generic;
    using System.Linq;

    #endregion

    public class UriMatchList
    {
        #region Constants and Fields

        private IList<UriPattern> _exceptionPatterns;

        private IList<UriPattern> _matchPatterns;

        private IList<UriPattern> _patterns;

        #endregion

        #region Constructors and Destructors

        public UriMatchList(IEnumerable<UriPattern> patterns)
        {
            this.Patterns = patterns;
        }

        #endregion

        #region Properties

        protected IEnumerable<UriPattern> Patterns
        {
            get
            {
                return this._patterns;
            }
            set
            {
                this._patterns = value.ToList();
                this._exceptionPatterns = this._patterns.Where(p => p.ExceptionRule).ToList();
                this._matchPatterns = this._patterns.Where(p => !p.ExceptionRule && !p.IsComment && !p.NotSupported).ToList();
            }
        }

        #endregion

        #region Public Methods

        public UriPatternMatchType Match(RequestProperties properties, out UriPattern matchedPattern)
        {
            var result = UriPatternMatchType.NonMatch;
            UriPattern output = null;
            Parallel.ForEach(this._exceptionPatterns, (p, state) => { 
                if (p.Match(properties) == UriPatternMatchType.Exception)
                {
                    state.Stop();
                    output = p;
                    result = UriPatternMatchType.Exception;
                }
            });

            if (result == UriPatternMatchType.Exception)
            {
                matchedPattern = output;
                return result;
            }

            Parallel.ForEach(this._matchPatterns, (p, state) =>
            {
                if (p.Match(properties) == UriPatternMatchType.Match)
                {
                    state.Stop();
                    output = p;
                    result = UriPatternMatchType.Match;
                }
            });

            matchedPattern = output;

            return result;
        }

        #endregion
    }
}