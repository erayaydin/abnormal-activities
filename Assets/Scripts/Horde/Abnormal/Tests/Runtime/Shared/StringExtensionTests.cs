using System;
using Horde.Abnormal.Shared;
using NUnit.Framework;

namespace Horde.Abnormal.Tests.Runtime.Shared
{
    [TestFixture]
    public class StringExtensionTests
    {
        [Test]
        public void ToTitleCaseEmptyStringReturnsEmptyString()
        {
            Assert.AreEqual("", "".ToTitleCase());
        }

        [Test]
        public void ToTitleCaseSingleWordReturnsCapitalizedWord()
        {
            Assert.AreEqual("Hello", "hello".ToTitleCase());
        }

        [Test]
        public void ToTitleCaseMultipleWordsReturnsCapitalizedWords()
        {
            Assert.AreEqual("Hello World", "hello world".ToTitleCase());
        }

        [Test]
        public void ToTitleCaseMixedCaseStringReturnsOriginalString()
        {
            Assert.AreEqual("Hello World", "Hello WOrld".ToTitleCase());
        }

        [Test]
        public void ToTitleCaseNullStringThrowsArgumentNullException()
        {
            string input = null;
            
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => input.ToTitleCase());
        }
    }
}