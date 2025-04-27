using Pinta.Core;

namespace ArsKaliGlitches;

[Mono.Addins.Extension]
public sealed class ArsKaliGlitchesExtension : IExtension
{
	public void Initialize ()
	{
		PintaCore.Effects.RegisterEffect (new ChromaticAberrationEffect (PintaCore.Services));
		PintaCore.Effects.RegisterEffect (new ColoredArtifactsEffect (PintaCore.Services));
	}

	public void Uninitialize ()
	{
		PintaCore.Effects.UnregisterInstanceOfEffect (typeof (ChromaticAberrationEffect));
		PintaCore.Effects.UnregisterInstanceOfEffect (typeof (ColoredArtifactsEffect));
	}
}