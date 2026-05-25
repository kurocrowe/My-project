using UnityEngine;

public class MosquitoSwarmEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem mosquitoParticles;
    [SerializeField] private Camera targetCamera;

    void Start()
    {
        if (mosquitoParticles == null)
            mosquitoParticles = GetComponent<ParticleSystem>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        ParticleSystemRenderer renderer =
            mosquitoParticles.GetComponent<ParticleSystemRenderer>();

        Material yellowMaterial = new Material(Shader.Find("Sprites/Default"));
        yellowMaterial.color = Color.yellow;

        renderer.material = yellowMaterial;

        float height = targetCamera.orthographicSize * 2f;
        float width = height * targetCamera.aspect;

        transform.position =
            targetCamera.transform.position +
            targetCamera.transform.forward * 5f;

        var main = mosquitoParticles.main;
        main.startLifetime = 6f;
        main.startSpeed = 0.5f;
        main.startSize = 0.06f;
        main.maxParticles = 250;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;

        var emission = mosquitoParticles.emission;
        emission.rateOverTime = 60f;

        var shape = mosquitoParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(width, height, 0.1f);

        var velocity = mosquitoParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var noise = mosquitoParticles.noise;
        noise.enabled = true;
        noise.strength = 1.2f;
        noise.frequency = 3.5f;
        noise.scrollSpeed = 2f;

        var color = mosquitoParticles.colorOverLifetime;
        color.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.yellow, 0f),
                new GradientColorKey(Color.yellow, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.1f),
                new GradientAlphaKey(1f, 0.85f),
                new GradientAlphaKey(0f, 1f)
            }
        );

        color.color = gradient;

        mosquitoParticles.Play();
    }
}