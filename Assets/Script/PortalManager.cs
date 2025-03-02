using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//ステンシルの比較条件を操作する際に必要
using UnityEngine.Rendering;

public class PortalManager : MonoBehaviour
{
    //クリッピング平面として使用するオブジェクト
    [SerializeField] Transform clippingPlaneTransform;
    //クリッピング平面の法線方向（1:表側 -1:裏側）
    [SerializeField] float clippingDirection = 1f;
    //移動先の世界の3Dモデルをまとめたオブジェクト
    [SerializeField] GameObject worldObject;
    //上記オブジェクトのマテリアル(描画設定ファイル)を保持するために使用
    List<Material> worldMaterials = new List<Material>();
    //クリッピングが有効かどうか
    private bool clippingEnabled = false;
    //現在の表示モード
    bool isARMode = true;
    //ゲートに表と裏どちらから入るか (1:表側から入る -1:裏側から入る)
    float enteringSide;
    
    // Start is called before the first frame update
    void Start()
    {
        //移動先の3DモデルのRendererを取得
        Renderer[] renderers = worldObject.GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renderers)
        {
            //マテリアルを取得
            Material material = renderer.sharedMaterial;
            //既に他のオブジェクトから取得したマテリアルでなければリストに追加
            if (!worldMaterials.Contains(material))
            {
                worldMaterials.Add(material);
                
                // クリッピング平面のシェーダープロパティを設定
                if (material.HasProperty("_ClipPlaneEnabled"))
                {
                    material.SetFloat("_ClipPlaneEnabled", 0); // 初期状態では無効
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (clippingEnabled)
        {
            UpdateClippingPlane();
        }
    }
    
    // クリッピング平面の情報をマテリアルに設定
    void UpdateClippingPlane()
    {
        if (clippingPlaneTransform == null) return;
        
        // 平面の法線と位置を取得
        Vector3 planeNormal = clippingPlaneTransform.forward * clippingDirection;
        Vector3 planePosition = clippingPlaneTransform.position;
        
        // 平面の方程式: ax + by + cz + d = 0 の形式で
        // (a,b,c) = 法線ベクトル, d = -法線ベクトル・平面上の点
        Vector4 planeEquation = new Vector4(
            planeNormal.x,
            planeNormal.y,
            planeNormal.z,
            -Vector3.Dot(planeNormal, planePosition)
        );
        
        foreach (Material material in worldMaterials)
        {
            if (material.HasProperty("_ClipPlane"))
            {
                material.SetVector("_ClipPlane", planeEquation);
            }
        }
    }
    
    // クリッピングの有効/無効を切り替え
    void SetClippingEnabled(bool enabled, bool isOutside = true)
    {
        clippingEnabled = enabled;
        
        foreach (Material material in worldMaterials)
        {
            if (material.HasProperty("_ClipPlaneEnabled"))
            {
                material.SetFloat("_ClipPlaneEnabled", enabled ? 1 : 0);
            }
            
            if (material.HasProperty("_ClipSide"))
            {
                // 1: 外側をクリップ、0: 内側をクリップ
                material.SetFloat("_ClipSide", isOutside ? 1 : 0);
            }
        }
        
        if (enabled)
        {
            UpdateClippingPlane();
        }
    }
    
    //他のオブジェクトが接触したときに呼ばれる
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Portal Entered");  
        //カメラの座標をゲートを原点にしたローカル座標に変換
        Vector3 localPos = transform.InverseTransformPoint(Camera.main.transform.position);
        //カメラがゲートの中心から横方向に0.5以上離れている場合は無視。(Quadは-0.5<=x<=0.5で定義されている)
        if(Mathf.Abs(localPos.x)>0.5f) return; 
        //Alwaysを指定してWorldを常に表示
        SetStencilComparison(CompareFunction.Always);  
        //ゲートに接触したらクリッピング処理をオン
        //裏か表を+-で表現
        enteringSide = Mathf.Sign(localPos.z);
        //条件に応じたクリッピング設定
        bool clipOutside = (isARMode && enteringSide<0)||(!isARMode&&enteringSide>0);
        SetClippingEnabled(true, clipOutside);
    }
    
    //接触終了時に呼ばれる
    void OnTriggerExit(Collider other)
    {
        Debug.Log("Portal Exited");
        //カメラの座標をゲートを原点にしたローカル座標に変換
        Vector3 localPos = transform.InverseTransformPoint(Camera.main.transform.position);
        //裏か表を+-で表現
        float exitingSide = Mathf.Sign(localPos.z);
        if(isARMode){//現在ARモード：
            if(exitingSide!=enteringSide){//入った方向と逆から出たならVRモードに切り替え
                SetStencilComparison(CompareFunction.NotEqual);
                isARMode = false;
            }
            else
            {//入った方向と同じ方向から出たならARモードのまま
                SetStencilComparison(CompareFunction.Equal);
            }
        }
        else{//現在VRモード：
            if(exitingSide!=enteringSide){//入った方向と逆から出たならARモードに切り替え
                SetStencilComparison(CompareFunction.Equal);
                isARMode = true;
            }
            else
            {//入った方向と同じ方向から出たならVRモードのまま
                SetStencilComparison(CompareFunction.NotEqual);
            }
        }
        //ゲートから離れたらクリッピング処理をオフ
        SetClippingEnabled(false);
    }
    
    //アプリ終了時にEditor内の表示をARモードに戻しておく
    void OnDestroy()
    {
        SetStencilComparison(CompareFunction.Equal);
    }
    
    //引数で受け取った設定でステンシルの比較条件を変更する
    void SetStencilComparison(CompareFunction mode){
        //移動先の3Dモデルのマテリアルを取得
        foreach(Material material in worldMaterials)
        {
            //ステンシルの比較条件を変更
            material.SetInt("_StencilComparison", (int)mode);
        }
    }
}