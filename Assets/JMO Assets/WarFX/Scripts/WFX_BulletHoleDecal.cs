using System.Collections;
using UnityEngine;

/**
 *	Handles the bullet hole decals:
 *	- Sets a random frame
 *	- Fades the material according to the defined lifetime
 *	- Optionally rotates the decal
 *	
 *	(c) 2015, Jean Moreno
**/
[RequireComponent(typeof(MeshFilter))]
public class WFX_BulletHoleDecal : MonoBehaviour {
    private static readonly Vector2[] quadUVs = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };

    public float lifetime = 10f;
    public float fadeoutpercent = 80;
    public Vector2 frames;
    public bool randomRotation = false;
    public bool deactivate = false;

    private float life;
    private float fadeout;
    private Color color;
    private float orgAlpha;

    private void Awake() {
        color = GetComponent<Renderer>().material.GetColor("_TintColor");
        orgAlpha = color.a;
    }

    private void OnEnable() {
        //Random UVs
        var random = Random.Range(0, (int)(frames.x * frames.y));
        var fx = (int)(random % frames.x);
        var fy = (int)(random / frames.y);
        //Set new UVs
        var meshUvs = new Vector2[4];
        for (var i = 0; i < 4; i++) {
            meshUvs[i].x = (quadUVs[i].x + fx) * (1.0f / frames.x);
            meshUvs[i].y = (quadUVs[i].y + fy) * (1.0f / frames.y);
        }
        GetComponent<MeshFilter>().mesh.uv = meshUvs;

        //Random rotate
        if (randomRotation) {
            transform.Rotate(0f, 0f, Random.Range(0f, 360f), Space.Self);
        }

        //Start lifetime coroutine
        life = lifetime;
        fadeout = life * (fadeoutpercent / 100f);
        color.a = orgAlpha;
        GetComponent<Renderer>().material.SetColor("_TintColor", color);
        StopAllCoroutines();
        StartCoroutine("holeUpdate");
    }

    private IEnumerator holeUpdate() {
        while (life > 0f) {
            life -= Time.deltaTime;
            if (life <= fadeout) {
                color.a = Mathf.Lerp(0f, orgAlpha, life / fadeout);
                GetComponent<Renderer>().material.SetColor("_TintColor", color);
            }

            yield return null;
        }
    }
}
