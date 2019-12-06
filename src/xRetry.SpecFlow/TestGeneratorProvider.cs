using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.CodeDom;
using TechTalk.SpecFlow.Generator.UnitTestProvider;
using xRetry.SpecFlow.Parsers;

namespace xRetry.SpecFlow
{
    public class TestGeneratorProvider : NUnit3TestGeneratorProvider
    {
        private readonly IRetryTagParser _retryTagParser;

        public TestGeneratorProvider(CodeDomHelper codeDomHelper, IRetryTagParser retryTagParser) 
            : base(codeDomHelper)
        {
            _retryTagParser = retryTagParser;
        }

        public new void SetTestMethodCategories(TestClassGenerationContext generationContext, CodeMemberMethod testMethod, IEnumerable<string> scenarioCategories)
        {
            // Prevent multiple enumerations
            scenarioCategories = scenarioCategories.ToList();

            base.SetTestMethodCategories(generationContext, testMethod, scenarioCategories);

            string strRetryTag = getRetryTag(scenarioCategories);
            if (strRetryTag != null)
            {
                RetryTag retryTag = _retryTagParser.Parse(strRetryTag);

                // Add the Retry attribute
                CodeDomHelper.AddAttribute(testMethod, $"NUnit.Framework.RetryAttribute({retryTag?.MaxRetries :: 5})");

            }
        }

        private string getRetryTag(IEnumerable<string> tags) =>
            tags.FirstOrDefault(t =>
                t.StartsWith(Constants.RETRY_TAG, StringComparison.OrdinalIgnoreCase) &&
                // Is just "retry", or is "retry("... for params
                (t.Length == Constants.RETRY_TAG.Length || t[Constants.RETRY_TAG.Length] == '('));
    }
}
