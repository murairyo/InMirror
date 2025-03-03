using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//ステンシルの比較条件を操作する際に必要
using UnityEngine.Rendering;

public class PortalManager : MonoBehaviour
{
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
    
    // クリッピング平面コンポーネントへの参照を追加
    [SerializeField] ClippingPlane_Origin clippingPlaneComponent;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("ポータルマネージャーの初期化を開始します");
        //移動先の3DモデルのRendererを取得
        Renderer[] renderers = worldObject.GetComponentsInChildren<Renderer>();
        Debug.Log("検出されたレンダラーの数: " + renderers.Length);
        
        // マテリアルのリストを作成
        List<Material> materialsToClip = new List<Material>();
        
        foreach(Renderer renderer in renderers)
        {
            //マテリアルを取得
            Material material = renderer.sharedMaterial;
            //既に他のオブジェクトから取得したマテリアルでなければリストに追加
            if (!worldMaterials.Contains(material))
            {
                worldMaterials.Add(material);
                materialsToClip.Add(material);
                Debug.Log("マテリアルを追加: " + material.name);
                
                // クリッピング平面のシェーダープロパティを設定
                if (material.HasProperty("_ClipPlaneEnabled"))
                {
                    material.SetFloat("_ClipPlaneEnabled", 0); // 初期状態では無効
                    Debug.Log("_ClipPlaneEnabledプロパティを設定しました");
                }
                else
                {
                    Debug.Log("警告: マテリアルに_ClipPlaneEnabledプロパティがありません");
                }
                
                // ステンシル設定を確認
                if (material.HasProperty("_StencilComp"))
                {
                    Debug.Log("_StencilCompプロパティが存在します。現在の値: " + material.GetInt("_StencilComp"));
                }
                else
                {
                    Debug.Log("警告: マテリアルに_StencilCompプロパティがありません");
                }
            }
        }
        
        // クリッピング平面コンポーネントにマテリアルを登録
        if (clippingPlaneComponent != null)
        {
            clippingPlaneComponent.clippedMaterials = materialsToClip.ToArray();
            // 初期状態では無効化
            clippingPlaneComponent.enabled = false;
            Debug.Log("クリッピング平面コンポーネントにマテリアルを登録しました");
        }
        else
        {
            Debug.Log("警告: クリッピング平面コンポーネントが設定されていません");
        }
        
        // 初期状態を明示的に設定
        SetClippingEnabled(false);
        SetStencilComparison(CompareFunction.Equal);
        Debug.Log("ポータルマネージャーの初期化が完了しました");
    }

    // Update is called once per frame
    void Update()
    {
        // クリッピング関連のコードを削除
    }
    
    // クリッピングの有効/無効を切り替え
    void SetClippingEnabled(bool enabled, bool isOutside = true)
    {
        clippingEnabled = enabled;
        Debug.Log("クリッピングを " + (enabled ? "有効" : "無効") + " に設定します。クリップ方向: " + (isOutside ? "外側" : "内側"));
        
        // ClippingPlane_Originコンポーネントを使用
        if (clippingPlaneComponent != null)
        {
            // クリッピング方向を設定
            clippingPlaneComponent.invertClipping = !isOutside;
            // コンポーネントの有効/無効を切り替え
            clippingPlaneComponent.enabled = enabled;
            Debug.Log("クリッピング平面コンポーネントを " + (enabled ? "有効" : "無効") + " に設定しました");
        }
        else
        {
            Debug.Log("警告: クリッピング平面コンポーネントが設定されていません");
        }
    }
    
    //他のオブジェクトが接触したときに呼ばれる
    void OnTriggerEnter(Collider other)
    {
        // カメラかプレイヤーのみ反応するように確認
        if (!other.CompareTag("Player") && !other.CompareTag("MainCamera"))
        {
            Debug.Log("ポータルに接触したオブジェクトはPlayerまたはMainCameraではありません: " + other.tag);
            return;
        }
        
        Debug.Log("ポータルに入りました: " + other.gameObject.name);  
        //カメラの座標をゲートを原点にしたローカル座標に変換
        Vector3 localPos = transform.InverseTransformPoint(Camera.main.transform.position);
        Debug.Log("カメラのローカル座標: " + localPos);
        
        //カメラがゲートの中心から横方向に0.5以上離れている場合は無視。(Quadは-0.5<=x<=0.5で定義されている)
        if(Mathf.Abs(localPos.x)>0.5f) 
        {
            Debug.Log("カメラがゲートの横方向範囲外です。処理をスキップします");
            return; 
        }
        
        //Alwaysを指定してWorldを常に表示
        Debug.Log("ステンシル比較条件をAlwaysに設定します");
        SetStencilComparison(CompareFunction.Always);  
        
        //ゲートに接触したらクリッピング処理をオン
        //裏か表を+-で表現
        enteringSide = Mathf.Sign(localPos.z);
        Debug.Log("ゲートに入る方向: " + (enteringSide > 0 ? "表側から" : "裏側から"));
        
        //条件に応じたクリッピング設定
        bool clipOutside = (isARMode && enteringSide<0)||(!isARMode&&enteringSide>0);
        Debug.Log("現在のモード: " + (isARMode ? "AR" : "VR") + "、クリップ方向: " + (clipOutside ? "外側" : "内側"));
        SetClippingEnabled(true, clipOutside);
    }
    
    //接触終了時に呼ばれる
    void OnTriggerExit(Collider other)
    {
        // カメラかプレイヤーのみ反応するように確認
        if (!other.CompareTag("Player") && !other.CompareTag("MainCamera"))
        {
            return;
        }
        
        Debug.Log("ポータルから出ました");
        //カメラの座標をゲートを原点にしたローカル座標に変換
        Vector3 localPos = transform.InverseTransformPoint(Camera.main.transform.position);
        Debug.Log("カメラのローカル座標: " + localPos);
        
        //裏か表を+-で表現
        float exitingSide = Mathf.Sign(localPos.z);
        Debug.Log("ゲートから出る方向: " + (exitingSide > 0 ? "表側へ" : "裏側へ"));
        Debug.Log("入った方向: " + (enteringSide > 0 ? "表側から" : "裏側から"));
        
        if(isARMode){//現在ARモード：
            if(exitingSide!=enteringSide){//入った方向と逆から出たならVRモードに切り替え
                Debug.Log("ARモードからVRモードに切り替えます");
                SetStencilComparison(CompareFunction.NotEqual);
                isARMode = false;
            }
            else
            {//入った方向と同じ方向から出たならARモードのまま
                Debug.Log("ARモードのままです");
                SetStencilComparison(CompareFunction.Equal);
            }
        }
        else{//現在VRモード：
            if(exitingSide!=enteringSide){//入った方向と逆から出たならARモードに切り替え
                Debug.Log("VRモードからARモードに切り替えます");
                SetStencilComparison(CompareFunction.Equal);
                isARMode = true;
            }
            else
            {//入った方向と同じ方向から出たならVRモードのまま
                Debug.Log("VRモードのままです");
                SetStencilComparison(CompareFunction.NotEqual);
            }
        }
        //ゲートから離れたらクリッピング処理をオフ
        SetClippingEnabled(false);
    }
    
    //アプリ終了時にEditor内の表示をARモードに戻しておく
    void OnDestroy()
    {
        Debug.Log("ポータルマネージャーを破棄します。ARモードに戻します");
        SetStencilComparison(CompareFunction.Equal);
    }
    
    //引数で受け取った設定でステンシルの比較条件を変更する
    void SetStencilComparison(CompareFunction mode){
        Debug.Log("ステンシル比較条件を変更: " + mode);
        //移動先の3Dモデルのマテリアルを取得
        foreach(Material material in worldMaterials)
        {
            //ステンシルの比較条件を変更
            if (material.HasProperty("_StencilComp"))
            {
                material.SetInt("_StencilComp", (int)mode);
                Debug.Log("マテリアル " + material.name + " の_StencilCompを " + mode + " に設定しました");
            }
            else
            {
                Debug.Log("警告: マテリアル " + material.name + " に_StencilCompプロパティがありません");
            }
        }
    }
}