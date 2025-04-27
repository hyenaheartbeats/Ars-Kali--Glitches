using System;
using System.Threading.Tasks;
using Cairo;
using Pinta.Core;
using Pinta.Gui.Widgets;
using Mono.Addins;
using Pinta.Effects;

namespace ArsKaliGlitches;

public sealed class ColoredArtifactsEffect : BaseEffect
{
	private readonly IChromeService chrome;
	private readonly IWorkspaceService workspace;
	private readonly UnaryPixelOp id;
	internal ColoredArtifactsEffect (IServiceProvider services)
	{
		chrome = services.GetService<IChromeService> ();
		workspace = services.GetService<IWorkspaceService> ();
		EffectData = new ColoredArtifactsData ();
		id = new UnaryPixelOps.Identity ();
	}

	public override string Name
		=> AddinManager.CurrentLocalizer.GetString ("Colored Artifacts");

	public override string EffectMenuCategory
		=> AddinManager.CurrentLocalizer.GetString ("Render");

	public override bool IsConfigurable => true;

	public override bool IsTileable => false;

	public override Task<bool> LaunchConfiguration ()
	{
		return chrome.LaunchSimpleEffectDialog (
			workspace,
			this,
			AddinManager.CurrentLocalizer);
	}

	public ColoredArtifactsData Data => (ColoredArtifactsData) EffectData!;

	private static double RandomBetween(Random rand, double min, double max)
	{
		return min + rand.NextDouble() * (max - min);
	}

	private static int RandomBetween(Random rand, int min, int max)
	{
		return rand.Next(min, max + 1);
	}


	public override void Render (
		ImageSurface source,
		ImageSurface destination,
		ReadOnlySpan<RectangleI> rois)
	{
		Random rand = new Random(Data.Seed.Value);

		ImageSurface artifactedImage = new ImageSurface(source.Format, source.Width, source.Height);
		using (Context ctx = new Context(artifactedImage))
		{
			ctx.SetSourceSurface(source, 0, 0);
			ctx.Paint();

			for (int i = 0; i < Data.numberArtifacts; i++)
			{
				double rw = source.Width * RandomBetween(rand, Data.minWidth, Data.maxWidth);
				double rh = source.Height * RandomBetween(rand, Data.minHeight, Data.maxHeight);

				double rx = rand.NextDouble() * (source.Width - rw);
				double ry = rand.NextDouble() * (source.Height - rh);

				double rr = rand.NextDouble();
				double rg = rand.NextDouble();
				double rb = rand.NextDouble();
				double ra = RandomBetween(rand, Data.minAlpha, Data.maxAlpha) / 255.0;

				ctx.SetSourceRgba(rr, rg, rb, ra);
				ctx.Rectangle(rx, ry, rw, rh);
				ctx.Fill();
			}
		}

		id.Apply(destination, artifactedImage, rois);
	}

	public sealed class ColoredArtifactsData : EffectData
	{
		[Caption ("Number of artifacts"), MinimumValue (1), MaximumValue(2048)]
		public int numberArtifacts = 128;

		[Caption ("Minimum artifact alpha"), MinimumValue (0), MaximumValue(255)]
		public int minAlpha = 64;

		[Caption ("Maximum artifact alpha"), MinimumValue (0), MaximumValue(255)]
		public int maxAlpha = 255;

		[Caption ("Maximum artifact height (% of image height)"), DigitsValue (2), IncrementValue (0.01), MinimumValue (0), MaximumValue (1)]
		public double maxHeight = 0.5;
		
		[Caption ("Minimum artifact height (% of image height)"), DigitsValue (2), IncrementValue (0.01), MinimumValue (0), MaximumValue (1)]
		public double minHeight = 0.2;

		[Caption ("Maximum artifact width (% of image width)"), DigitsValue (2), IncrementValue (0.01), MinimumValue (0), MaximumValue (1)]
		public double maxWidth = 0.5;

		[Caption ("Minimum artifact width (% of image width)"), DigitsValue (2), IncrementValue (0.01), MinimumValue (0), MaximumValue (1)]
		public double minWidth = 0.2;

		public RandomSeed Seed;
	}
}