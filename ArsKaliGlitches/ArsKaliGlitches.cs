using Pinta.Core;

namespace ArsKaliGlitches;

[Mono.Addins.Extension]
public sealed class ArsKaliGlitchesExtension : IExtension
{
	public void Initialize ()
	{
		PintaCore.Effects.RegisterEffect (new ChromaticAberrationEffect (PintaCore.Services));
		PintaCore.Effects.RegisterEffect (new ColoredArtifactsEffect (PintaCore.Services));
		PintaCore.Effects.RegisterEffect (new RowSliceEffect (PintaCore.Services));
		PintaCore.Effects.RegisterEffect (new AdjustmentNoiseEffect (PintaCore.Services));
		PintaCore.Effects.RegisterEffect (new ScanlinesEffect (PintaCore.Services));
		PintaCore.Effects.RegisterEffect (new PixelDragEffect (PintaCore.Services));
	}

	public void Uninitialize ()
	{
		PintaCore.Effects.UnregisterInstanceOfEffect<ChromaticAberrationEffect> ();
		PintaCore.Effects.UnregisterInstanceOfEffect<ColoredArtifactsEffect> ();
		PintaCore.Effects.UnregisterInstanceOfEffect<RowSliceEffect> ();
		PintaCore.Effects.UnregisterInstanceOfEffect<AdjustmentNoiseEffect> ();
		PintaCore.Effects.UnregisterInstanceOfEffect<ScanlinesEffect> ();
		PintaCore.Effects.UnregisterInstanceOfEffect<PixelDragEffect> ();
	}
}
