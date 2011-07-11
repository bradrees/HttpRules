// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UriMatcher.cs" company="">
//   
// </copyright>
// <summary>
//   The uri pattern.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HttpRulesCore
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    #endregion

    /* * is wildcard
    * @@ is exception 
    * | is match at start/end of filter (regex ^ and $)
    * || match domain
    * ^ is wildcard for separator characters (or end). [~0-9a-zA-Z-.%]
    * ! is comment
    * $ is option
    * $~ is not option
    * domain option uses | as OR operator
    * /../ is a regular expression
    * 
    * ## is element hiding
    * domains can be separated by , (domain1.example,domain2.example,domain3.example##*.sponsor)
    * ~ can be applied to domains
    * # is element hiding (depricated)
    */

    /// <summary>
    /// The uri pattern match type.
    /// </summary>
    [Flags]
    public enum UriPatternMatchType : byte
    {
        /// <summary>
        /// The pattern does not match.
        /// </summary>
        NonMatch = 0,

        /// <summary>
        /// The pattern matches.
        /// </summary>
        Match = 1,

        /// <summary>
        /// The pattern matches, and is flagged as an exception rule (i.e. stop further processing and return non-match).
        /// </summary>
        Exception = 2
    }

    /// <summary>
    /// The uri pattern token type.
    /// </summary>
    public enum UriPatternTokenType
    {
        /// <summary>
        /// A string literal
        /// </summary>
        String = 0,

        /// <summary>
        /// Wildcard (*). // TODO: Is this 1->N or 0->N
        /// </summary>
        Wildcard,

        /// <summary>
        /// Exception (@@).
        /// </summary>
        Exception,

        /// <summary>
        /// Match at the start or end of a string (|)
        /// </summary>
        MatchStartEnd,

        /// <summary>
        /// Match only the domain (||).
        /// </summary>
        MatchDomain,

        /// <summary>
        /// Match any separator characters, or end of string (^). [~0-9a-zA-Z-.%]
        /// </summary>
        Separator,

        /// <summary>
        /// Options for matching ($).
        /// </summary>
        Option,

        /// <summary>
        /// Options for not matching (~).
        /// </summary>
        NotOption,

        /// <summary>
        /// Options separator (,).
        /// </summary>
        OptionSeparator,

        /// <summary>
        /// A regular expression (/../).
        /// </summary>
        RegularExpression,

        /// <summary>
        /// Hide an element (##). (Not supported)
        /// </summary>
        ElementHiding,

        /// <summary>
        /// Depricated method for hiding elements (#). (Not supported)
        /// </summary>
        ElementHidingDepricated,

        /// <summary>
        /// A CSS selector used for hiding elements (occurs after ##). (Not supported) 
        /// </summary>
        CssSelector,

        /// <summary>
        /// This rule is a comment.
        /// </summary>
        Comment
    }

    /// <summary>
    /// The uri pattern token.
    /// </summary>
    public struct UriPatternToken
    {
        #region Constants and Fields

        /// <summary>
        /// The token type.
        /// </summary>
        public UriPatternTokenType Type;

        /// <summary>
        /// The value of the token, if applicable.
        /// </summary>
        public string Value;

        #endregion
    }

    /// <summary>
    /// The match.
    /// </summary>
    public struct Match
    {
        #region Constants and Fields

        /// <summary>
        /// The index.
        /// </summary>
        public int Index;

        /// <summary>
        /// The length.
        /// </summary>
        public int Length;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Match"/> struct.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <param name="length">
        /// The length.
        /// </param>
        public Match(int index, int length)
        {
            this.Index = index;
            this.Length = length;
        }

        #endregion
    }

    /// <summary>
    /// The uri pattern matching engine.
    /// </summary>
    public class UriPattern
    {
        #region Constants and Fields

        /// <summary>
        /// The pattern tokens.
        /// </summary>
        private readonly IList<UriPatternToken> _patternTokens;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UriPattern"/> class.
        /// </summary>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        public UriPattern(string pattern)
        {
            this.Pattern = pattern;
            this.MatchCase = StringComparison.OrdinalIgnoreCase;
            this.IncludeDomains = new List<string>();
            this.ExcludeDomains = new List<string>();
            this.FilterOptions = new List<Func<RequestProperties, bool>>();

            // Tokenize and remove empty strings.
            this._patternTokens = this.Tokenize(pattern).Where(t => t.Type != UriPatternTokenType.String || !String.IsNullOrWhiteSpace(t.Value)).ToList();

            if (this._patternTokens.Count > 0 && this._patternTokens.First().Type == UriPatternTokenType.Comment)
            {
                this.IsComment = true;
                return;
            }

            var optionsIndex = this._patternTokens.IndexOf(this._patternTokens.FirstOrDefault(t => t.Type == UriPatternTokenType.Option));
            if (optionsIndex > 0)
            {
                this.ParseOptions(this._patternTokens.Skip(optionsIndex + 1).ToList());
                this._patternTokens = this._patternTokens.Take(optionsIndex).ToList();
            }
            else if (optionsIndex == 0)
            {
                throw new ArgumentException("Uri pattern should not start with an option flag.");
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether ExceptionRule.
        /// </summary>
        public bool ExceptionRule { get; private set; }

        /// <summary>
        /// Gets ExcludeDomains.
        /// </summary>
        public IList<string> ExcludeDomains { get; private set; }

        /// <summary>
        /// Gets FilterOptions.
        /// </summary>
        public IList<Func<RequestProperties, bool>> FilterOptions { get; private set; }

        /// <summary>
        /// Gets IncludeDomains.
        /// </summary>
        public IList<string> IncludeDomains { get; private set; }

        /// <summary>
        /// Gets a value indicating whether IsComment.
        /// </summary>
        public bool IsComment { get; private set; }

        /// <summary>
        /// Gets MatchCase.
        /// </summary>
        public StringComparison MatchCase { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this rule is supported by this matcher. Will always return NonMatch if true.
        /// </summary>
        public bool NotSupported { get; private set; }

        /// <summary>
        /// Gets Pattern.
        /// </summary>
        public string Pattern { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks to see if the request matches any of the patterns.
        /// </summary>
        /// <param name="properties">
        /// The request properties.
        /// </param>
        /// <param name="pattern">
        /// The patterns to match.
        /// </param>
        /// <returns>
        /// <c>true</c> if any of the patterns were a match.
        /// </returns>
        public static bool Match(RequestProperties properties, IEnumerable<UriPattern> pattern)
        {
            foreach (var result in pattern.Select(uriPattern => uriPattern.Match(properties)))
            {
                switch (result)
                {
                    case UriPatternMatchType.Exception:
                        return false;
                    case UriPatternMatchType.Match:
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the request matches the pattern.
        /// </summary>
        /// <param name="properties">
        /// The request properties.
        /// </param>
        /// <returns>
        /// The match type.
        /// </returns>
        public UriPatternMatchType Match(RequestProperties properties)
        {
            return (!this.FilterOptions.Any() || this.FilterOptions.Select(f => f.Invoke(properties)).Any()) ? this.Match(properties.Uri) : UriPatternMatchType.NonMatch;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Is the <see cref="Uri"/> a match for this pattern.
        /// </summary>
        /// <param name="uri">
        /// The URI to check.
        /// </param>
        /// <returns>
        /// The type of match.
        /// </returns>
        protected UriPatternMatchType Match(Uri uri)
        {
            if (this.IsComment || this.NotSupported)
            {
                return UriPatternMatchType.NonMatch;
            }

            var uriString = uri.OriginalString;
            var stringIndex = 0; // how far through the string are we.
            var matchReturn = UriPatternMatchType.Match;
            var comparisonType = this.MatchCase;
            bool matchStart = false, matchEnd = false;
            var matchIndices = new List<IEnumerable<Match>>();
            var tokenCount = this._patternTokens.Count();
            for (var i = 0; i < tokenCount; i++)
            {
                var token = this._patternTokens[i];
                switch (token.Type)
                {
                    case UriPatternTokenType.String:
                        var stringMatches = IndicesOf(uriString, token.Value, stringIndex, comparisonType).ToList();
                        if (!stringMatches.Any())
                        {
                            return UriPatternMatchType.NonMatch;
                        }

                        stringIndex = stringMatches.First() + token.Value.Length;
                        matchIndices.Add(stringMatches.Select(s => new Match(s, token.Value.Length)));
                        break;
                    case UriPatternTokenType.Wildcard:

                        // Add From here to the end, minus the sum length of any other tokens. If the length of the remaining token is greater than the remaining length we can't match.
                        var rangeCount = (uriString.Length - stringIndex) - this._patternTokens.Skip(i).Sum(t => t.Value.Length);
                        if (rangeCount < 0)
                        {
                            return UriPatternMatchType.NonMatch;
                        }

                        matchIndices.Add(Enumerable.Range(stringIndex, rangeCount + 1).Select(s => new Match(s, 0)));
                        break;
                    case UriPatternTokenType.Exception:
                        matchReturn = UriPatternMatchType.Exception;
                        break;
                    case UriPatternTokenType.MatchStartEnd:
                        if (i == tokenCount - 1)
                        {
                            // match end
                            matchEnd = true;
                        }
                        else
                        {
                            // match start
                            matchStart = true;
                        }

                        break;
                    case UriPatternTokenType.MatchDomain:
                        if (!(uri.Host.Equals(token.Value, StringComparison.OrdinalIgnoreCase) || uri.Host.EndsWith("." + token.Value, StringComparison.OrdinalIgnoreCase)))
                        {
                            return UriPatternMatchType.NonMatch;
                        }

                        stringIndex = uriString.IndexOf(token.Value) + token.Value.Length;
                        matchIndices.Add(new[] { new Match(0, stringIndex) });
                        break;
                    case UriPatternTokenType.Separator:
                        var separators = IndicesOfSeparators(uriString, stringIndex).ToList();
                        if (!separators.Any() && tokenCount == i + 1)
                        {
                            // If the last token is a separator then it can count.
                            continue;
                        }

                        if (!separators.Any())
                        {
                            return UriPatternMatchType.NonMatch;
                        }

                        stringIndex++;
                        matchIndices.Add(separators.Select(s => new Match(s, 1)));
                        break;
                    case UriPatternTokenType.NotOption:
                    case UriPatternTokenType.OptionSeparator:

                        // these are not valid here, so take them as string literals.
                        matchIndices.Add(new[] { new Match(stringIndex, 1) });
                        break;
                    case UriPatternTokenType.RegularExpression:
                        return Regex.IsMatch(uriString, token.Value) ? matchReturn : UriPatternMatchType.NonMatch;

                    case UriPatternTokenType.ElementHiding:
                    case UriPatternTokenType.ElementHidingDepricated:
                    case UriPatternTokenType.CssSelector:
                        this.NotSupported = true;
                        return UriPatternMatchType.NonMatch;
                    case UriPatternTokenType.Option:
                        throw new ArgumentException("The token list should not have options contained inside it.");

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // check for start and end matches
            if (matchStart)
            {
                var firstMatch = matchIndices.FirstOrDefault();
                if (firstMatch != null)
                {
                    matchIndices[0] = firstMatch.Where(m => m.Index == 0);
                    if (!matchIndices[0].Any())
                    {
                        return UriPatternMatchType.NonMatch;
                    }
                }
            }

            if (matchEnd)
            {
                var lastMatch = matchIndices.LastOrDefault();
                if (lastMatch != null)
                {
                    matchIndices[0] = lastMatch.Where(m => m.Index + m.Length == uriString.Length);
                    if (!matchIndices[0].Any())
                    {
                        return UriPatternMatchType.NonMatch;
                    }
                }
            }

            return matchIndices.First().Any(m => FindSequence(uriString, matchIndices, m.Index, 0)) ? matchReturn : UriPatternMatchType.NonMatch;
        }

        /// <summary>
        /// Search through list looking for a consecutive list of numbers.
        /// If you encounter a wildcard recursively find an entire match pattern.
        /// No need to check for start and end, these are already covered.
        /// </summary>
        /// <param name="uriString">
        /// The uri pattern to search.
        /// </param>
        /// <param name="matchIndices">
        /// The list of indices to try and match.
        /// </param>
        /// <param name="index">
        /// The index in the string to check for a match.
        /// </param>
        /// <param name="matchIndex">
        /// The index of the match list to check.
        /// </param>
        /// <returns>
        /// <c>true</c> if there is a match, else false.
        /// </returns>
        private static bool FindSequence(string uriString, List<IEnumerable<Match>> matchIndices, int index, int matchIndex)
        {
            var result = false;
            if (matchIndex >= matchIndices.Count)
            {
                // All matches have been checked, terminate recursion.
                return true;
            }

            if (index >= uriString.Length)
            {
                // At the end of the string, no match and no further recursion.
                return false;
            }

            var possibleMatches = matchIndices[matchIndex].Count();
            for (var i = 0; i < possibleMatches; i++)
            {
                var match = matchIndices[matchIndex].ElementAt(i);
                if (match.Length == 0)
                {
                    result = FindSequence(uriString, matchIndices, match.Index, matchIndex + 1);
                }
                else
                {
                    // Check for a match at the next value, if true continue.
                    if (match.Index == index)
                    {
                        result = FindSequence(uriString, matchIndices, match.Index + match.Length, matchIndex + 1);
                    }
                }

                if (result)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Indiceses the of value in the input (like IndexOf, except it returns all instances).
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <param name="comparison">
        /// The comparison.
        /// </param>
        /// <returns>
        /// The indices of the value in the input.
        /// </returns>
        private static IEnumerable<int> IndicesOf(string input, string value, int startIndex = 0, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (startIndex >= input.Length)
            {
                yield break;
            }

            var index = startIndex;
            do
            {
                var result = input.IndexOf(value, index, comparison);
                if (result != -1)
                {
                    index = result;
                    yield return index;
                }
                else
                {
                    yield break;
                }
            }
            while (++index <= input.Length);
        }

        /// <summary>
        /// Indiceses the of separator characters.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <param name="startIndex">
        /// The start index.
        /// </param>
        /// <returns>
        /// The indices of the separator characters.
        /// </returns>
        private static IEnumerable<int> IndicesOfSeparators(string input, int startIndex)
        {
            if (startIndex >= input.Length)
            {
                yield break;
            }

            var index = startIndex;
            do
            {
                if (IsSeparator(input[index]))
                {
                    yield return index;
                }
            }
            while (++index < input.Length);
        }

        /// <summary>
        /// Determines whether the specified char is a separator.
        /// </summary>
        /// <param name="c">
        /// The character.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified char is a separator; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsSeparator(char c)
        {
            return !((c >= 'a' && c <= 'z') || c == '.' || (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || c == '-' || c == '%' || c > 127);
        }

        /// <summary>
        /// The parse options.
        /// </summary>
        /// <param name="tokens">
        /// The tokens.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        private void ParseOptions(IEnumerable<UriPatternToken> tokens)
        {
            var options = tokens.Split(t => t.Type == UriPatternTokenType.OptionSeparator);
            foreach (var option in options)
            {
                var inverse = false;
                var inverseDomain = false;
                var parsingDomains = false;
                foreach (var token in option)
                {
                    switch (token.Type)
                    {
                        case UriPatternTokenType.NotOption:
                            if (parsingDomains)
                            {
                                inverseDomain = true;
                            }
                            else
                            {
                                inverse = true;
                            }

                            break;
                        case UriPatternTokenType.MatchStartEnd:
                            if (!parsingDomains)
                            {
                                throw new ArgumentException("Error parsing options");
                            }

                            inverseDomain = false;
                            break;
                        case UriPatternTokenType.String:
                            if (parsingDomains)
                            {
                                (inverseDomain ? this.ExcludeDomains : this.IncludeDomains).Add(token.Value);
                            }
                            else
                            {
                                switch (token.Value.ToLowerInvariant())
                                {
                                    case "third-party":
                                        var inverseTp = inverse;
                                        this.FilterOptions.Add(r => inverseTp ^ !(r.Referer.HasValue() && r.Referer.Host.EqualsCIS(r.Uri.Host)));
                                        break;
                                    case "match-case":
                                        this.MatchCase = inverse ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                                        break;
                                    case "domain":
                                        parsingDomains = true;
                                        var inverseD = inverse;
                                        this.FilterOptions.Add(
                                            r => inverseD ^ !(this.IncludeDomains.Any(d => r.Uri.IsDomainOrSubdomain(d)) && this.ExcludeDomains.Any(d => r.Uri.IsDomainOrSubdomain(d))));
                                        break;
                                    case "stylesheet":
                                        var inverseCss = inverse;
                                        this.FilterOptions.Add(r => inverseCss ^ !(r.Uri.HasFileExtension("css") || r.Accept.StartsWith("text/css")));
                                        break;
                                    case "script":
                                        var inverseJs = inverse;
                                        this.FilterOptions.Add(r => inverseJs ^ !r.Uri.HasFileExtension("js"));
                                        break;
                                    case "image": // .png, .gif, .jpg, .jpeg, .svg 
                                        var inverseImg = inverse;
                                        this.FilterOptions.Add(r => inverseImg ^ !r.Uri.HasFileExtension("jpg", "png", "gif", "jpeg", "svg"));
                                        break;
                                    case "object": // .swf, .jar, .class, .xap
                                        var inverseObj = inverse;
                                        this.FilterOptions.Add(r => inverseObj ^ !r.Uri.HasFileExtension("swf", "xap", "jar", "class"));
                                        break;
                                    case "object_subrequest":
                                        var inverseObjSub = inverse;
                                        this.FilterOptions.Add(r => inverseObjSub ^ !r.Referer.HasFileExtension("swf", "xap", "jar", "class"));
                                        break;
                                    case "dtd": // .dtd
                                        var inverseDtd = inverse;
                                        this.FilterOptions.Add(r => inverseDtd ^ !r.Uri.HasFileExtension("dtd"));
                                        break;
                                    case "background": // NS
                                    case "xbl": // NS
                                    case "ping": // NS
                                    case "xmlhttprequest": // Partial support?
                                    case "subdocument": // ?
                                    case "document": // NS
                                    case "elemhide": // NS
                                    case "other": // NS
                                    case "donottrack": // NS - this hsould add the header X-Do-Not-Track, however won't work with the current architecture without some reworks
                                        this.FilterOptions.Add(r => false);
                                        this.NotSupported = true;
                                        break;
                                    case "collapse": // Not supported, but not critical
                                        break;
                                }
                            }

                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        /// <summary>
        /// Tokenizes the specified pattern.
        /// </summary>
        /// <param name="pattern">
        /// The pattern.
        /// </param>
        /// <returns>
        /// A list of tokens.
        /// </returns>
        private IEnumerable<UriPatternToken> Tokenize(string pattern)
        {
            if (String.IsNullOrEmpty(pattern))
            {
                yield break;
            }

            var tempString = new List<char>();
            Func<string> getTempString = () =>
                {
                    var s = new string(tempString.ToArray());
                    tempString.Clear();
                    return s;
                };
            Func<char, bool> isValidDomainChar = c => (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '.' || (c >= 'A' && c <= 'Z') || c == '-' || c > 127;
            var parseOptions = false;
            for (var i = 0; i < pattern.Length; i++)
            {
                var current = pattern[i];
                var next = i < pattern.Length - 1 ? pattern[i + 1] : (char)0;

                switch (current)
                {
                    case '*':
                        yield return new UriPatternToken { Type = UriPatternTokenType.String, Value = getTempString() };
                        yield return new UriPatternToken { Type = UriPatternTokenType.Wildcard, Value = String.Empty };
                        break;
                    case '@':
                        if (next == '@')
                        {
                            yield return new UriPatternToken { Type = UriPatternTokenType.Exception, Value = "@@" };
                            this.ExceptionRule = true;
                            i++;
                        }
                        else
                        {
                            tempString.Add(current);
                        }

                        break;
                    case '|':
                        yield return new UriPatternToken { Type = UriPatternTokenType.String, Value = getTempString() };

                        if (next == '|')
                        {
                            i += 2;
                            var startIndex = i;
                            if (!isValidDomainChar(pattern[i]))
                            {
                                throw new ArgumentException("Uri pattern must have a valid domain if using the match domain argument.");
                            }

                            while (i < pattern.Length && isValidDomainChar(pattern[i]))
                            {
                                i++;
                            }

                            yield return new UriPatternToken { Type = UriPatternTokenType.MatchDomain, Value = pattern.Substring(startIndex, i - startIndex) };
                            if (i < pattern.Length)
                            {
                                i--; // need to go back one, as we have advanced to the point of an invalid character.
                            }
                        }
                        else
                        {
                            yield return new UriPatternToken { Type = UriPatternTokenType.MatchStartEnd, Value = "|" };
                        }

                        break;
                    case '$':
                        yield return new UriPatternToken { Type = UriPatternTokenType.String, Value = getTempString() };
                        yield return new UriPatternToken { Type = UriPatternTokenType.Option, Value = "$" };
                        parseOptions = true;
                        break;
                    case '~':
                        if (parseOptions)
                        {
                            yield return new UriPatternToken { Type = UriPatternTokenType.String, Value = getTempString() };
                            yield return new UriPatternToken { Type = UriPatternTokenType.NotOption, Value = "~" };
                        }
                        else
                        {
                            tempString.Add(current);
                        }

                        break;
                    case '/':
                        if (i == 0 && pattern.Length > 1 && pattern[pattern.Length - 1] == '/')
                        {
                            yield return new UriPatternToken { Type = UriPatternTokenType.RegularExpression, Value = pattern.Substring(1, pattern.Length - 2) };
                            i = pattern.Length;
                        }
                        else
                        {
                            tempString.Add(current);
                        }

                        break;
                    case '^':
                        yield return new UriPatternToken { Type = UriPatternTokenType.String, Value = getTempString() };
                        yield return new UriPatternToken { Type = UriPatternTokenType.Separator, Value = "^" };
                        break;
                    case '=':

                        // This is used during domain options as the separator between the option and the domain list. (domain=example.com)
                        yield return new UriPatternToken { Type = UriPatternTokenType.String, Value = getTempString() };
                        yield return new UriPatternToken { Type = UriPatternTokenType.String, Value = "=" };
                        break;
                    case '#':
                        if (next == '#')
                        {
                            yield return new UriPatternToken { Type = UriPatternTokenType.String, Value = getTempString() };
                            yield return new UriPatternToken { Type = UriPatternTokenType.ElementHiding, Value = "##" };
                            i++;
                            yield return new UriPatternToken { Type = UriPatternTokenType.CssSelector, Value = pattern.Substring(i + 1, pattern.Length - i) };
                            i = pattern.Length;
                        }
                        else
                        {
                            yield return new UriPatternToken { Type = UriPatternTokenType.String, Value = getTempString() };
                            yield return new UriPatternToken { Type = UriPatternTokenType.ElementHidingDepricated, Value = pattern.Substring(i, pattern.Length - i) };
                        }

                        break;
                    case '!':
                        yield return new UriPatternToken { Type = UriPatternTokenType.String, Value = getTempString() };
                        yield return new UriPatternToken { Type = UriPatternTokenType.Comment, Value = "!" };
                        yield break;
                    case ',':
                        yield return new UriPatternToken { Type = UriPatternTokenType.String, Value = getTempString() };
                        yield return new UriPatternToken { Type = UriPatternTokenType.OptionSeparator, Value = "," };
                        break;
                    default:
                        tempString.Add(current);
                        break;
                }
            }

            yield return new UriPatternToken { Type = UriPatternTokenType.String, Value = getTempString() };
        }

        #endregion
    }
}