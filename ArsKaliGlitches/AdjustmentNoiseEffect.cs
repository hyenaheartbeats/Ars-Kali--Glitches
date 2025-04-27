using System;
using System.Threading.Tasks;
using Cairo;
using Pinta.Core;
using Pinta.Gui.Widgets;
using Mono.Addins;
using Pinta.Effects;

namespace ArsKaliGlitches;

public sealed class AdjustmentNoiseEffect : BaseEffect
{
	private readonly IChromeService chrome;
	private readonly IWorkspaceService workspace;
	internal AdjustmentNoiseEffect (IServiceProvider services)
	{
		chrome = services.GetService<IChromeService> ();
		workspace = services.GetService<IWorkspaceService> ();
		EffectData = new AdjustmentNoiseData ();
	}

	public override string Name
		=> AddinManager.CurrentLocalizer.GetString ("Adjustment Noise");

	public override string EffectMenuCategory
		=> AddinManager.CurrentLocalizer.GetString ("Noise");

	public override bool IsConfigurable => true;

	public override bool IsTileable => false;

	public override Task<bool> LaunchConfiguration ()
	{
		return chrome.LaunchSimpleEffectDialog (
			workspace,
			this,
			AddinManager.CurrentLocalizer);
	}

	public AdjustmentNoiseData Data => (AdjustmentNoiseData) EffectData!;

	private static int RandomBetween(Random rand, int min, int max)
	{
		return rand.Next(min, max + 1);
	}

	private static int AdjustChannel(int val, Random rand)
	{
		if (val > 239) {
			return RandomBetween(rand, 239, 255);
		} 
		else if (val < 16) {
			return RandomBetween(rand, 1, 16);
		} else {
			return RandomBetween(rand, val - 16, val + 16);
		}
	}

	protected override ColorBgra Render (in ColorBgra pixel)
	{
		Random rand = new Random();
		int xr = AdjustChannel(pixel.R, rand);
		int xg = AdjustChannel(pixel.G, rand);
		int xb = AdjustChannel(pixel.B, rand);

		return ColorBgra.FromBgra(
			b: Utility.ClampToByte(xb),
			g: Utility.ClampToByte(xg),
			r: Utility.ClampToByte(xr),
			a: pixel.A
		);
		
	}

	public sealed class AdjustmentNoiseData : EffectData
	{
		public RandomSeed Seed;
	}
}