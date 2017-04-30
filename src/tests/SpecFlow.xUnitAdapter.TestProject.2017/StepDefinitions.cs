using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using Xunit;

namespace SpecFlow.xUnitAdapter.TestProject._2017
{
    [Binding]
    public class StepDefinitions
    {
        private List<int> numbers = new List<int>();
        private int number;

        [Given(@"I have entered (.*) into the calculator")]
        public void GivenIHaveEnteredIntoTheCalculator(int number)
        {
            this.numbers.Add(number);
        }

        [When(@"I press add")]
        public void WhenIPressAdd()
        {
            this.number = this.numbers.Sum();
        }

        [Then(@"the result should be (.*) on the screen")]
        public void ThenTheResultShouldBeOnTheScreen(int number)
        {
            Assert.Equal(number, this.number);
        }
    }
}
