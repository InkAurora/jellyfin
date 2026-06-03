using System;
using Jellyfin.Api.Controllers;
using Jellyfin.Api.Helpers;
using Xunit;

namespace Jellyfin.Api.Tests.Controllers
{
    public class DynamicHlsControllerTests
    {
        [Theory]
        [MemberData(nameof(GetSegmentLengths_Success_TestData))]
        public void GetSegmentLengths_Success(long runtimeTicks, int segmentlength, double[] expected)
        {
            var res = DynamicHlsController.GetSegmentLengthsInternal(runtimeTicks, segmentlength);
            Assert.Equal(expected.Length, res.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], res[i]);
            }
        }

        [Fact]
        public void CreateAdaptiveBitrateVariants_ObeysMaxBitrateAndNoUpscale()
        {
            var variants = DynamicHlsHelper.CreateAdaptiveBitrateVariants(
                4200000,
                128000,
                8000000,
                10000000,
                1280,
                720,
                1280,
                720);

            Assert.All(variants, variant =>
            {
                Assert.True(variant.TotalBitrate <= 4200000);
                Assert.True(variant.Width <= 1280);
                Assert.True(variant.Height <= 720);
            });
            Assert.Contains(variants, variant => variant.VideoBitrate == 4000000 && variant.Height == 720);
            Assert.DoesNotContain(variants, variant => variant.Height > 720);
        }

        public static TheoryData<long, int, double[]> GetSegmentLengths_Success_TestData()
        {
            var data = new TheoryData<long, int, double[]>();
            data.Add(0, 6, Array.Empty<double>());
            data.Add(
                TimeSpan.FromSeconds(3).Ticks,
                6,
                new double[] { 3 });
            data.Add(
                TimeSpan.FromSeconds(6).Ticks,
                6,
                new double[] { 6 });
            data.Add(
                TimeSpan.FromSeconds(3.3333333).Ticks,
                6,
                new double[] { 3.3333333 });
            data.Add(
                TimeSpan.FromSeconds(9.3333333).Ticks,
                6,
                new double[] { 6, 3.3333333 });

            return data;
        }
    }
}
