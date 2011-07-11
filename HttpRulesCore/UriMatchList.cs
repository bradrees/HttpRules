namespace HttpRulesCore
{
    #region Using Directives

    using System.Collections.Generic;
    using System.Linq;

    #endregion

    public class UriMatchList
    {
        #region Constants and Fields

        private IEnumerable<UriPattern> _exceptionPatterns;

        private IEnumerable<UriPattern> _matchPatterns;

        private IEnumerable<UriPattern> _patterns;

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
            matchedPattern = this._exceptionPatterns.AsParallel().FirstOrDefault(p => p.Match(properties) == UriPatternMatchType.Exception);
            if (matchedPattern != null)
            {
                return UriPatternMatchType.Exception;
            }

            matchedPattern = this._matchPatterns.AsParallel().FirstOrDefault(p => p.Match(properties) == UriPatternMatchType.Match);

            return matchedPattern != null ? UriPatternMatchType.Match : UriPatternMatchType.NonMatch;
        }

        #endregion
    }
}