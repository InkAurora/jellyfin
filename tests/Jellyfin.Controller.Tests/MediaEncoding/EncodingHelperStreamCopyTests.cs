using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Streaming;
using MediaBrowser.Controller.Subtitles;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using IConfigurationManager = MediaBrowser.Common.Configuration.IConfigurationManager;

namespace Jellyfin.Controller.Tests.MediaEncoding;

public class EncodingHelperStreamCopyTests
{
    private readonly EncodingHelper _helper;

    public EncodingHelperStreamCopyTests()
    {
        var mediaEncoder = new Mock<IMediaEncoder>();
        mediaEncoder
            .Setup(i => i.CanEncodeToAudioCodec(It.IsAny<string>()))
            .Returns<string>(codec => string.Equals(codec, "aac", System.StringComparison.OrdinalIgnoreCase));

        _helper = new EncodingHelper(
            Mock.Of<IApplicationPaths>(),
            mediaEncoder.Object,
            Mock.Of<ISubtitleEncoder>(),
            new ConfigurationBuilder().Build(),
            Mock.Of<IConfigurationManager>(),
            Mock.Of<IPathManager>());
    }

    [Theory]
    [InlineData("ac3")]
    [InlineData("eac3")]
    public void TryStreamCopy_HlsFmp4VideoTranscodeWithAc3Audio_TranscodesAudio(string audioCodec)
    {
        var state = CreateVideoTranscodeState("mp4", audioCodec);

        _helper.TryStreamCopy(state, new EncodingOptions());

        Assert.Equal("hevc", state.OutputVideoCodec);
        Assert.Equal("aac", state.OutputAudioCodec);
    }

    [Fact]
    public void TryStreamCopy_HlsTsVideoTranscodeWithAc3Audio_CopiesAudio()
    {
        var state = CreateVideoTranscodeState("ts", "ac3");

        _helper.TryStreamCopy(state, new EncodingOptions());

        Assert.Equal("hevc", state.OutputVideoCodec);
        Assert.Equal("copy", state.OutputAudioCodec);
    }

    [Fact]
    public void TryStreamCopy_HlsFmp4VideoCopyWithAc3Audio_TranscodesAudio()
    {
        var state = CreateVideoTranscodeState("mp4", "ac3");
        state.OutputVideoCodec = "h264";
        state.SupportedVideoCodecs = new[] { "h264" };

        _helper.TryStreamCopy(state, new EncodingOptions());

        Assert.Equal("copy", state.OutputVideoCodec);
        Assert.Equal("aac", state.OutputAudioCodec);
    }

    [Fact]
    public void TryStreamCopy_HlsFmp4WithAc3Audio_UsesAacEvenWhenClientPrefersAc3()
    {
        var state = CreateVideoTranscodeState("mp4", "ac3");
        state.OutputAudioCodec = "ac3";
        state.SupportedAudioCodecs = new[] { "ac3" };

        _helper.TryStreamCopy(state, new EncodingOptions());

        Assert.Equal("aac", state.OutputAudioCodec);
    }

    private static EncodingJobInfo CreateVideoTranscodeState(string segmentContainer, string audioCodec)
    {
        return new EncodingJobInfo(TranscodingJobType.Hls)
        {
            BaseRequest = new StreamingRequestDto
            {
                SegmentContainer = segmentContainer
            },
            AudioStream = new MediaStream
            {
                Type = MediaStreamType.Audio,
                Codec = audioCodec,
                Channels = 2,
                SampleRate = 48000,
                BitRate = 192000
            },
            VideoStream = new MediaStream
            {
                Type = MediaStreamType.Video,
                Codec = "h264",
                IsAVC = true,
                BitRate = 5000000
            },
            IsVideoRequest = true,
            OutputAudioCodec = "aac",
            OutputVideoCodec = "hevc",
            SupportedAudioCodecs = new[] { "aac", audioCodec },
            SupportedVideoCodecs = new[] { "hevc" }
        };
    }
}
