using System;
using System.Threading.Tasks;
using Cairo;
using Pinta.Core;
using Pinta.Gui.Widgets;
using Mono.Addins;
using Pinta.Effects;
using System.Collections.Generic;

namespace ArsKaliGlitches;

public sealed class PixelDragEffect : BaseEffect
{
	private readonly IChromeService chrome;
	private readonly IWorkspaceService workspace;
	private readonly UnaryPixelOp id;
	internal PixelDragEffect (IServiceProvider services)
	{
		chrome = services.GetService<IChromeService> ();
		workspace = services.GetService<IWorkspaceService> ();
		EffectData = new PixelDragData ();
		id = new UnaryPixelOps.Identity ();
	}

	public override string Name
		=> AddinManager.CurrentLocalizer.GetString ("Pixel Drag");

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

	public PixelDragData Data => (PixelDragData) EffectData!;

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
		
		int iterations = Data.dragCount; // changed from being a percentage
		// when it was a percentage things got Real Fucked Up

		int maxDrag = 0;
		int minDrag = 0;
		if (Data.dragDirection == "X")
		{
			maxDrag = (int)(source.Width * Data.maxDragLength);
			minDrag = (int)(source.Width * Data.minDragLength);
		}
		else
		{
			maxDrag = (int)(source.Height * Data.maxDragLength);
			minDrag = (int)(source.Height * Data.minDragLength);
		}

		// surprisingly painful to figure out
		// thnankfully i am high
		// and i already wrote an image glitcher with similar functionality when i was 16 
		// (it's out there if you want to find it but i'm not going to link to it because i was a horrible teenager)
		// so that's like 60% of the work done for me
		// 40% was reading the source for the twist effect
		// and another 120% was my wife optimizing it

		// does anyone read these things?
		// if so can you buy me some arby's
		// i need the sodium
		using (Context ctx = new Context(destination))
		{
			ctx.SetSourceSurface(source, 0, 0);
			ctx.Paint();
		}
		
		Span<byte> data = source.GetData();
		Span<byte> destinationData = destination.GetData();
		
		if (Data.dragDirection == "X")
		{
			RenderX(source, data, destinationData, iterations, minDrag, maxDrag, rand);
		}
		else
		{
			// done by twi
			// because otherwise the whole thing slows to a crawl
			// oops
			RenderY(source, data, destinationData, iterations, minDrag, maxDrag, rand);
		}
		
		destination.MarkDirty();
	}
	
	private void RenderX(
		ImageSurface source,
		Span<byte> data,
		Span<byte> destinationData,
		int iterations,
		int minDrag,
		int maxDrag,
		Random rand)
	{
		for (int i = 0; i < iterations; i++)
		{
			int x = rand.Next(source.Width);
			int y = rand.Next(source.Height);
			int index = y * source.Stride + x * 4;
			
			byte[] pixcolor = {data[index], data[index + 1], data[index + 2], data[index + 3]};
			int dragLength = RandomBetween(rand, minDrag, maxDrag);
			int basePosition = y * source.Stride + x * 4;
			
			for (int j = 0; j < dragLength; j++)
			{
				int xx = x + j;
				
				if (xx >= source.Width)
				{
					break;
				}
				
				int position = basePosition + j * 4;
				destinationData[position] = pixcolor[0];
				destinationData[position + 1] = pixcolor[1];
				destinationData[position + 2] = pixcolor[2];
				destinationData[position + 3] = pixcolor[3];
			}
		}
	}
	
	private void RenderY(
		ImageSurface source,
		Span<byte> data,
		Span<byte> destinationData,
		int iterations,
		int minDrag,
		int maxDrag,
		Random rand)
	{
		const int bs = 64;
		int[] xs = new int[bs];
		int[] ys = new int[bs];
		byte[][] pixcolors = new byte[bs][];
		int[] lengths = new int[bs];
		
		for (int bstart = 0; bstart < iterations; bstart += bs)
		{
			int currentbs = Math.Min(bs, iterations - bstart);
			for (int i = 0; i < currentbs; i++)
			{
				xs[i] = rand.Next(source.Width);
				ys[i] = rand.Next(source.Height);
				
				int index = ys[i] * source.Stride + xs[i] * 4;
				pixcolors[i] = new byte[] {data[index], data[index + 1], data[index + 2], data[index + 3]};
				
				lengths[i] = RandomBetween(rand, minDrag, maxDrag);
			}
			
			for (int i = 0; i < currentbs; i++)
			{
				int x = xs[i];
				int y = ys[i];
				byte[] pixcolor = pixcolors[i];
				int dragLength = lengths[i];
				
				for (int j = 0; j < dragLength; j++)
				{
					int yy = y - j;
					
					if (yy < 0)
					{
						break;
					}
					
					int position = yy * source.Stride + x * 4;
					destinationData[position] = pixcolor[0];
					destinationData[position + 1] = pixcolor[1];
					destinationData[position + 2] = pixcolor[2];
					destinationData[position + 3] = pixcolor[3];
				}
			}
		}
	}

	public sealed class PixelDragData : EffectData
	{
		public static Dictionary<string, object> dragDirections = new Dictionary<string, object> () {
			{"X", 1}, {"Y", 2}
		};
		[Caption ("Drag direction"), StaticList("dragDirections")]
		public string dragDirection = "X";
		[Caption ("Min length to drag pixels (% of axis length)"), DigitsValue (3), IncrementValue (0.001), MinimumValue (0), MaximumValue (1)]
		public double minDragLength = 0.01;
		[Caption ("Max length to drag pixels (% of axis length)"), DigitsValue (3), IncrementValue (0.001), MinimumValue (0), MaximumValue (1)]
		public double maxDragLength = 0.01;

		[Caption ("# of pixels to drag"), MinimumValue (0), MaximumValue (4096)]
		public int dragCount = 512;

		public RandomSeed Seed;
	}
}
