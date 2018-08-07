using UnityEngine;

public class ParticlePainter : BasePainter
{
    [SerializeField]
    private new ParticleSystemRenderer renderer;

    public override void Refresh() =>
        renderer.material.color = Variable.Value.Fade(renderer.material.color.a);
}
