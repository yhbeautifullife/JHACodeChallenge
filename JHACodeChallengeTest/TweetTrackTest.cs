using System;
using Xunit;
using JHACodeChallenge;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace JHACodeChallengeTest
{
    public class TweetTrackTest
    {
        [Fact]
        public void FindEmojisInText_should_not_return_null()
        {
            TweetTrack track = new TweetTrack(null, null);
            List<string> lst = track.FindEmojisInText("");
            Assert.NotNull(lst);
        }

        [Theory]
        [InlineData("\uD83D\uDE00\uD83D\uDE0A\uD83D\uDE03#test", 3)]
        [InlineData("谢😒😒",2)]
        public void FindEmojisInText_shouldSearch(string text, int expected)
        {
            TweetTrack track = new TweetTrack(null, null);
            List<string> lst = track.FindEmojisInText(text);
            Assert.Equal(lst.Count, expected);
        }

    }
}
