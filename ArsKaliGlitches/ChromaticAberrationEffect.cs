using System;
using System.Threading.Tasks;
using Cairo;
using Pinta.Core;
using Pinta.Gui.Widgets;
using Mono.Addins;
using Pinta.Effects;

namespace ArsKaliGlitches;

public sealed class ChromaticAberrationEffect : BaseEffect
{
	private readonly IChromeService chrome;
	private readonly IWorkspaceService workspace;
	private readonly UnaryPixelOp blankChannelRed;
	private readonly UnaryPixelOp blankChannelGreen;
	private readonly UnaryPixelOp blankChannelBlue;
	private readonly UnaryPixelOp id;
	internal ChromaticAberrationEffect (IServiceProvider services)
	{
		chrome = services.GetService<IChromeService> ();
		workspace = services.GetService<IWorkspaceService> ();
		EffectData = new ChromaticAberrationData ();
		blankChannelRed = new UnaryPixelOps.SetChannel (2,0);
		blankChannelGreen = new UnaryPixelOps.SetChannel (1,0);
		blankChannelBlue = new UnaryPixelOps.SetChannel (0,0);
		id = new UnaryPixelOps.Identity ();
	}

	public override string Name
		=> AddinManager.CurrentLocalizer.GetString ("Chromatic Aberration");

	public override string EffectMenuCategory
		=> AddinManager.CurrentLocalizer.GetString ("Stylize");

	public override bool IsConfigurable => true;

	public override bool IsTileable => false;

	public override Task<bool> LaunchConfiguration ()
	{
		return chrome.LaunchSimpleEffectDialog (
			workspace,
			this,
			AddinManager.CurrentLocalizer);
	}

	public ChromaticAberrationData Data => (ChromaticAberrationData) EffectData!;

	private void DrawChannel (
		Context ctx,
		ImageSurface source,
		int osx,
		int osy,
		bool isTiled
	)
	{
		ctx.SetSourceRgba(0, 0, 0, 0);
		ctx.Rectangle(0, 0, source.Width, source.Height);
		ctx.Fill();
		if (isTiled)
		{
			for (int dx = -1; dx <= 1; dx++)
			{
				for (int dy = -1; dy <= 1; dy++)
				{
					double dsx = osx + dx * source.Width;
					double dsy = osy + dy * source.Height;

					ctx.SetSourceSurface(source, dsx, dsy);
					ctx.Paint();
				}
			}
		}
		else
		{
			ctx.SetSourceSurface(source, osx, osy);
			ctx.Paint();
		}
	}

	public override void Render (
		ImageSurface source,
		ImageSurface destination,
		ReadOnlySpan<RectangleI> rois)
	{
		ImageSurface imageRed = new ImageSurface(source.Format, source.Width, source.Height);
		ImageSurface imageGreen = new ImageSurface(source.Format, source.Width, source.Height);
		ImageSurface imageBlue = new ImageSurface(source.Format, source.Width, source.Height);

		int rsx = Data.Redshift.X - (source.Width / 2);
		int rsy = Data.Redshift.Y - (source.Height / 2);
		int bsx = Data.Blueshift.X - (source.Width / 2);
		int bsy = Data.Blueshift.Y - (source.Height / 2);
		int gsx = Data.Greenshift.X - (source.Width / 2);
		int gsy = Data.Greenshift.Y - (source.Height / 2);

		using (Context ctx = new Context(imageRed))
		{
			DrawChannel(ctx, source, rsx, rsy, Data.Tiledresult);
		}
		using (Context ctx = new Context(imageGreen))
		{
			DrawChannel(ctx, source, gsx, gsy, Data.Tiledresult);
		}
		using (Context ctx = new Context(imageBlue))
		{
			DrawChannel(ctx, source, bsx, bsy, Data.Tiledresult);
		}

		blankChannelGreen.Apply(imageRed, rois);
		blankChannelBlue.Apply(imageRed, rois);
		blankChannelRed.Apply(imageGreen, rois);
		blankChannelBlue.Apply(imageGreen, rois); 
		blankChannelGreen.Apply(imageBlue, rois);
		blankChannelRed.Apply(imageBlue, rois);

		using (Context ctx = new Context(imageRed))
		{
			ctx.Operator = Operator.Add;

			ctx.SetSourceSurface(imageGreen, 0, 0);
			ctx.Paint();
			ctx.SetSourceSurface(imageBlue, 0, 0);
			ctx.Paint();
		}

		id.Apply(destination, imageRed, rois);
	}

	public sealed class ChromaticAberrationData : EffectData
	{
		[Caption ("Red shift")]
		public PointI Redshift;
		
		[Caption ("Green shift")]
		public PointI Greenshift;

		[Caption ("Blue shift")]
		public PointI Blueshift;

		[Caption ("Tile result")]
		public bool Tiledresult = false;
	}
}