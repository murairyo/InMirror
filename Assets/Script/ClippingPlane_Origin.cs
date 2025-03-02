using UnityEngine;

public class ClippingPlane_Origin : MonoBehaviour
{
    [Tooltip("クリッピングする側を反転するかどうか")]
    public bool invertClipping = false;

    [Tooltip("クリッピングの影響を受けるマテリアル")]
    public Material[] clippedMaterials;

    protected int clipPlaneID;
    protected int clipPlaneSideID;
    private Vector4 clipPlane;
    private const string keywordName = "_CLIPPING_PLANE";

    protected virtual void OnEnable()
    {
        Initialize();
        UpdateShaderProperties();
    }

    protected virtual void OnDisable()
    {
        SetShaderPropertiesActive(false);
    }

    protected virtual void Update()
    {
        UpdateShaderProperties();
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (enabled)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(1.0f, 0.0f, 1.0f));
            Gizmos.DrawLine(Vector3.zero, Vector3.up * -0.5f);
        }
    }

    protected virtual void Initialize()
    {
        clipPlaneID = Shader.PropertyToID("_ClipPlane");
        clipPlaneSideID = Shader.PropertyToID("_ClipPlaneSide");
    }

    protected virtual void UpdateShaderProperties()
    {
        Vector3 up = transform.up;
        clipPlane = new Vector4(up.x, up.y, up.z, Vector3.Dot(up, transform.position));

        SetShaderPropertiesActive(true);
        
        foreach (Material material in clippedMaterials)
        {
            if (material != null)
            {
                material.SetVector(clipPlaneID, clipPlane);
                material.SetFloat(clipPlaneSideID, invertClipping ? -1.0f : 1.0f);
            }
        }
    }

    protected virtual void SetShaderPropertiesActive(bool active)
    {
        foreach (Material material in clippedMaterials)
        {
            if (material != null)
            {
                if (active)
                {
                    material.EnableKeyword(keywordName);
                }
                else
                {
                    material.DisableKeyword(keywordName);
                }
            }
        }
    }
}
