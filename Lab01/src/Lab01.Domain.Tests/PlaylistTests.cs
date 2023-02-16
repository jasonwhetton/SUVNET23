using Xunit;
namespace Lab01.Domain.Tests
{
    public class PlaylistTests
    {
        [Fact]
        public void Active_when_created()
        {
            var playlist = new Playlist();

            Assert.True(playlist.IsActive);
        }
    }
}
