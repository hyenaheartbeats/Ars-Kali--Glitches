using System;
using System.Threading.Tasks;
using Cairo;
using Pinta.Core;
using Pinta.Gui.Widgets;
using Mono.Addins;
using Pinta.Effects;

namespace ArsKaliGlitches;

public sealed class RowSliceEffect : BaseEffect
{
	private readonly IChromeService chrome;
	private readonly IWorkspaceService workspace;
	private readonly UnaryPixelOp id;
	internal RowSliceEffect (IServiceProvider services)
	{
		chrome = services.GetService<IChromeService> ();
		workspace = services.GetService<IWorkspaceService> ();
		EffectData = new RowSliceData ();
		id = new UnaryPixelOps.Identity ();
	}

	public override string Name
		=> AddinManager.CurrentLocalizer.GetString ("Row Slice");

	public override string EffectMenuCategory
		=> AddinManager.CurrentLocalizer.GetString ("Distort");

	public override bool IsConfigurable => true;

	public override bool IsTileable => false;

	public override Task<bool> LaunchConfiguration ()
	{
		return chrome.LaunchSimpleEffectDialog (
			workspace,
			this,
			AddinManager.CurrentLocalizer);
	}

	public RowSliceData Data => (RowSliceData) EffectData!;

	private static double RandomBetween(Random rand, double min, double max)
	{
		return min + rand.NextDouble() * (max - min);
	}

	public override void Render (
		ImageSurface source,
		ImageSurface destination,
		ReadOnlySpan<RectangleI> rois)
	{
		Random rand = new Random(Data.Seed.Value);
		int sliceHeight = source.Height / Data.numberSlices;
		double boundaryLeft = -(Data.leftShift * (source.Width / 2.0));
		double boundaryRight = (Data.rightShift * (source.Width / 2.0));

		ImageSurface slicedImage = new ImageSurface(source.Format, source.Width, sliceHeight);
		ImageSurface result = new ImageSurface(source.Format, source.Width, source.Height);

		using (Context ctx = new Context(result))
		{
			ctx.SetSourceSurface(source, 0, 0);
			ctx.Paint();
		}

		for (int i = 0; i < Data.numberSlices; i++)
		{
			int currentPosition = sliceHeight * i;
			double shift = RandomBetween(rand, boundaryLeft, boundaryRight);

			using (Context ctx = new Context(slicedImage))
			{
				ctx.Operator = Operator.Clear;
				ctx.Paint();
				ctx.Operator = Operator.Over;
				
				for (int sc = -1; sc <= 1; sc++)
				{
					double wrappedX = -shift + sc * source.Width;

					ctx.SetSourceSurface(source, wrappedX, -currentPosition);
					ctx.Paint();
				}
			}

			using (Context ctx = new Context(result))
			{
				ctx.SetSourceSurface(slicedImage, 0, currentPosition);
				ctx.Paint();
			}

		}

		id.Apply(destination, result, rois);
	}

	public sealed class RowSliceData : EffectData
	{
		[Caption ("Number of slices"), MinimumValue (1), MaximumValue(128)]
		public int numberSlices = 32;

		[Caption ("Leftmost shift (% of half image width)"), DigitsValue (2), IncrementValue (0.01), MinimumValue (0), MaximumValue (1)]
		public double leftShift = 0.5;
		
		[Caption ("Rightmost shift (% of half image width)"), DigitsValue (2), IncrementValue (0.01), MinimumValue (0), MaximumValue (1)]
		public double rightShift = 0.5;

		public RandomSeed Seed;
	}
}