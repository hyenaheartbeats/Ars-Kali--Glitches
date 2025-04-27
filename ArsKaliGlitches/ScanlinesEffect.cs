using System;
using System.Threading.Tasks;
using Cairo;
using Pinta.Core;
using Pinta.Gui.Widgets;
using Mono.Addins;
using Pinta.Effects;

namespace ArsKaliGlitches;

public sealed class ScanlinesEffect : BaseEffect
{
	private readonly IChromeService chrome;
	private readonly IWorkspaceService workspace;
	private readonly UnaryPixelOp id;
	internal ScanlinesEffect (IServiceProvider services)
	{
		chrome = services.GetService<IChromeService> ();
		workspace = services.GetService<IWorkspaceService> ();
		EffectData = new ScanlinesData ();
		id = new UnaryPixelOps.Identity ();
	}

	public override string Name
		=> AddinManager.CurrentLocalizer.GetString ("Scanlines");

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

	public ScanlinesData Data => (ScanlinesData) EffectData!;

	private static double RandomBetween(Random rand, double min, double max)
	{
		return min + rand.NextDouble() * (max - min);
	}

	public override void Render (
		ImageSurface source,
		ImageSurface destination,
		ReadOnlySpan<RectangleI> rois)
	{
		ImageSurface scanlineImage = new ImageSurface (source.Format, source.Width, source.Height);
		using (Context ctx = new Context(scanlineImage))
		{
			ctx.SetSourceSurface(source, 0, 0);
			ctx.Paint();

				if (Data.Scanlines)
				{
					ctx.Operator = Operator.Overlay;
					ctx.SetSourceRgba(0, 0, 0, 0.25);

					for (int y = 0; y < source.Height; y += 2)
					{
						ctx.Rectangle(0, y, source.Width, 1);
					}

					ctx.Fill();

					ctx.Operator = Operator.Multiply;

					for (int x = 0; x < source.Width; x += 3)
					{
						ctx.SetSourceRgba(1, 0, 0, 0.1);
						ctx.Rectangle(x, 0, 1, source.Height);
						ctx.Fill();

						ctx.SetSourceRgba(0, 1, 0, 0.1);
						ctx.Rectangle(x + 1, 0, 1, source.Height);
						ctx.Fill();

						ctx.SetSourceRgba(0, 0, 1, 0.1);
						ctx.Rectangle(x + 2, 0, 1, source.Height);
						ctx.Fill();
					}
				}

				ctx.Operator = Operator.Overlay;

				for (int y = 0; y < source.Height; y += 4)
				{
					if (Data.Red && y < source.Height)
					{
						ctx.SetSourceRgb(1, 0, 0);
						ctx.Rectangle(0, y, source.Width, 1);
						ctx.Fill();
					}

					if (Data.Blue && y + 2 < source.Height)
					{
						ctx.SetSourceRgb(0, 0, 1);
						ctx.Rectangle(0, y + 2, source.Width, 1);
						ctx.Fill();
					}

					if (Data.Green && y + 3 < source.Height)
					{
						ctx.SetSourceRgb(0, 1, 0);
						ctx.Rectangle(0, y + 3, source.Width, 1);
						ctx.Fill();
					}
				}
		}

		

		id.Apply(destination, scanlineImage, rois);
	}

	public sealed class ScanlinesData : EffectData
	{
		[Caption ("Scanlines")]
		public bool Scanlines = true;
		[Caption ("Red interlace lines")]
		public bool Red = true;
		[Caption ("Green interlace lines")]
		public bool Green = true;
		[Caption ("Blue interlace lines")]
		public bool Blue = true;
	}
}