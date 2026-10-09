// this one i wanted to do for a while now!!
// it was a pain!!!!!!!
// besides pinta's documentation being uhhhh.......... search the repo for specific code and pray
// there's a lot of ideas i wanted to implement from various sources
// the effect, of course comes from the wonderful john robbins/red ochre whos plugins for paint.net i've used since i was a wee lass
// https://forums.paint.net/topic/22126-psychocolour-in-red-ochre-plugin-pack/
// which, besides just looking at the results, all i had to go on was the small description of it given its closed source nature
// the actual gradient math comes from mark ransom on stack overflow
// https://stackoverflow.com/questions/22607043/color-gradient-algorithm
// while the effect isn't quite 1:1, and is slow on large images,,,
// it's pretty good as is and i think i'm pretty happy with the end result of trying to replicate it from scratch :>
using System;
using System.Threading.Tasks;
using Cairo;
using Pinta.Core;
using Pinta.Gui.Widgets;
using Mono.Addins;
using Pinta.Effects;

namespace ArsKaliGlitches;
public sealed class PsychocolorEffect : BaseEffect
{
    private readonly IChromeService chrome;
	private readonly IWorkspaceService workspace;

    internal PsychocolorEffect (IServiceProvider services)
	{
		chrome = services.GetService<IChromeService> ();
		workspace = services.GetService<IWorkspaceService> ();
		EffectData = new PsychocolorData ();
	}

    	public override string Name
		=> AddinManager.CurrentLocalizer.GetString ("Psychocolor");

	public override string EffectMenuCategory
		=> AddinManager.CurrentLocalizer.GetString ("Ars Kali: Glitches");

	public override bool IsConfigurable => true;

	public override bool IsTileable => false;

    public PsychocolorData Data => (PsychocolorData) EffectData!;

	public override Task<bool> LaunchConfiguration ()
	{
		return chrome.LaunchSimpleEffectDialog (
			workspace,
			this,
			AddinManager.CurrentLocalizer);
	}

    private const double gamma = 0.43;

    // math bullshit
    private static double FromSrgb(double x)
    {
        return x <= 0.04045
            ? x / 12.92
            : Math.Pow((x + 0.055) / 1.055, 2.4);
    }
    private static double ToSrgb(double x)
    {
        return x <= 0.0031308
            ? 12.92 * x
            : 1.055 * Math.Pow(x, 1.0 / 2.4) - 0.055;
    }
    private static double Lerp(double a, double b, double frac)
    {
        return a * (1.0 - frac) + b * frac;
    }

    private static double RandomBetween(Random rand, double min, double max)
	{
		return min + rand.NextDouble() * (max - min);
	}

    private static byte ToByte(double value)
    {
        return (byte)Math.Clamp((int)Math.Round(value * 255.0), 0, 255);
    }

    public override void Render (
		ImageSurface source,
		ImageSurface destination,
		ReadOnlySpan<RectangleI> rois)
    {
        Random rand = new Random(Data.rngSeed);

        // okay, so my interpretation of the effect:
        // i pick five colors at random and place them at 0, 64, 128, 192, and 255
        // then i use mark's algorithm to interpolate between them and create a gradient
        // then i take each pixel's luminance and use that to determine which color in the gradient to use for that pixel
        // "amplified colors" just increases the amount of colors used, as while developing i came across lots of different ways to tweak things :>
        Color[] colors = new Color[256];
        colors[0] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
        colors[64] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
        colors[128] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
        colors[192] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
        colors[255] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
        if (Data.amplifiedColors)
        {
            colors[16] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
            colors[32] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
            colors[48] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
            colors[80] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
            colors[96] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
            colors[112] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
            colors[144] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
            colors[160] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
            colors[176] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
            colors[208] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
            colors[224] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
            colors[240] = new Color(RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1), RandomBetween(rand, 0, 1));
        }

        int amplifiedEndPoint = (Data.amplifiedColors) ? 16 : 4;

        for (int i = 0; i < amplifiedEndPoint; i++)
        {
            // i dont know anything about the math
            // i just copied it
            int[] startIndexes;
            int[] endIndexes;
            if (Data.amplifiedColors)
            {
                startIndexes = [0, 16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240];
                endIndexes = [16, 32, 48, 64, 80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 255];
            } else
            {
                startIndexes = [0, 64, 128, 192];
                endIndexes = [64, 128, 192, 255];
            }
            int startIndex = startIndexes[i];
            int endIndex = endIndexes[i];

            double[] starter = [FromSrgb(colors[startIndex].R), FromSrgb(colors[startIndex].G), FromSrgb(colors[startIndex].B)];
            double[] ender = [FromSrgb(colors[endIndex].R), FromSrgb(colors[endIndex].G), FromSrgb(colors[endIndex].B)];
            double startBrightness = Math.Pow(starter[0] + starter[1] + starter[2], gamma);
            double endBrightness = Math.Pow(ender[0] + ender[1] + ender[2], gamma);

            for (int j = startIndex + 1; j < endIndex; j++)
            {
                double t;
                if (Data.solidColors)
                {
                    // really interesting bug (or rather different outcome) i found
                    t = (double)i / (endIndex - 1);
                } else {
                    // intended
                    t = (double)(j - startIndex) / (endIndex - startIndex);
                }
                
                double brightness = Math.Pow(Lerp(startBrightness, endBrightness, t), 1.0 / gamma);

                double r = Lerp(starter[0], ender[0], t);
                double g = Lerp(starter[1], ender[1], t);
                double b = Lerp(starter[2], ender[2], t);

                double sum = r + g + b;

                if (sum > 0.0)
                {
                    double scale = brightness / sum;
                    r *= scale;
                    g *= scale;
                    b *= scale;
                }

                colors[j] = new Color(Math.Clamp(ToSrgb(r), 0.0, 1.0), Math.Clamp(ToSrgb(g), 0.0, 1.0), Math.Clamp(ToSrgb(b), 0.0, 1.0));
            }

            int height = source.Height;
            int width = source.Width;
            int stride = source.Stride;
            Span<byte> data = source.GetData();
            Span<byte> destinationData = destination.GetData();

            for (int y = 0; y < height; y++)
            {
                int row = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int offset = row + x * 4;

                    byte b = data[offset];
                    byte g = data[offset + 1];
                    byte r = data[offset + 2];
                    byte a = data[offset + 3];

                    int tone = (int)(0.299 * r + 0.587 * g + 0.114 * b);
                    Color newcolor = colors[tone];
                    double alpha = a / 255.0;

                    destinationData[offset] = ToByte(newcolor.B * alpha);
                    destinationData[offset + 1] = ToByte(newcolor.G * alpha);
                    destinationData[offset + 2] = ToByte(newcolor.R * alpha);
                    destinationData[offset + 3] = a;
                }
            }
        }

        destination.MarkDirty();
    }

    public sealed class PsychocolorData : EffectData
    {
        // random seed as int instead of the actual randomseed type so results will be consistent
        // like the original plugin :>
        [Caption ("Seed"), MinimumValue (-65536), MaximumValue (65536),]
		public int rngSeed = 0;

        [Caption ("Solid Colors")]
        public bool solidColors = false;

        [Caption ("Amplified Colors")]
        public bool amplifiedColors = false;
    }
}