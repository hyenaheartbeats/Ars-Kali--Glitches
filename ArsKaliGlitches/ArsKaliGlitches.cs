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
		PintaCore.Effects.UnregisterInstanceOfEffect (typeof (ChromaticAberrationEffect));
		PintaCore.Effects.UnregisterInstanceOfEffect (typeof (ColoredArtifactsEffect));
		PintaCore.Effects.UnregisterInstanceOfEffect (typeof (RowSliceEffect));
		PintaCore.Effects.UnregisterInstanceOfEffect (typeof (AdjustmentNoiseEffect));
		PintaCore.Effects.UnregisterInstanceOfEffect (typeof (ScanlinesEffect));
		PintaCore.Effects.UnregisterInstanceOfEffect (typeof (PixelDragEffect));
	}
}