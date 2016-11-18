using UnityEngine;
using System.Collections;

public class UIParticleClip : MonoBehaviour
{
	public UIPanel m_panel;
	public string need_Change_Shader_Additive = "Particles/Additive";
	public string need_Change_Shader_Blend_Additive = "Particles/Alpha Blended";
	public Shader shader_Additive_ClipByUI;
	public Shader shader_Additive_Blend_ClipByUI;
	[HideInInspector]
	public Camera uiCamera;

	int m_panelSizeXProperty;
	int m_panelSizeYProperty;
	int m_panelCenterAndSharpnessProperty;

	void Start()
	{
		if(shader_Additive_ClipByUI == null || !shader_Additive_ClipByUI.isSupported ||
			shader_Additive_Blend_ClipByUI == null || !shader_Additive_Blend_ClipByUI.isSupported)
		{
			gameObject.SetActive(false);
			Debug.LogError("replace shader is null");
			return;
		}
		if (m_panel == null)
		{
			gameObject.SetActive(false);
			Debug.LogError("m_panel is null");
			return;
		}
		uiCamera = UIManager.Instance.UnqueUICamera;
		if(uiCamera == null)
		{
			gameObject.SetActive(false);
			Debug.LogError("uiCamera is null");
			return;
		}

		m_panelSizeXProperty = Shader.PropertyToID("_PanelSizeX");
		m_panelSizeYProperty = Shader.PropertyToID("_PanelSizeY");
		m_panelCenterAndSharpnessProperty = Shader.PropertyToID("_PanelCenterAndSharpness");

		UpdateClip(m_panel);
		m_panel.onClipMove += UpdateClip;
	}
	void UpdateClip(UIPanel panel)
	{
		if (panel && panel.hasClipping)
		{
			var soft = panel.clipSoftness;
			var sharpness = new Vector2(1000.0f, 1000.0f);

			if (soft.x > 0f)
			{
				sharpness.x = panel.baseClipRegion.z / soft.x;
			}
			if (soft.y > 0f)
			{
				sharpness.y = panel.baseClipRegion.w / soft.y;
			}

			Vector4 panelCenterAndSharpness;

			//var uiCamera = NGUIUtil.GetCamera();
			Debug.Assert(uiCamera != null);
			var panelWorldCorners = m_panel.worldCorners;
			var leftBottom = uiCamera.WorldToViewportPoint(panelWorldCorners[0]);
			var topRight = uiCamera.WorldToViewportPoint(panelWorldCorners[2]);
			var center = Vector3.Lerp(leftBottom, topRight, 0.5f);

			panelCenterAndSharpness.x = center.x;
			panelCenterAndSharpness.y = center.y;
			panelCenterAndSharpness.z = sharpness.x;
			panelCenterAndSharpness.w = sharpness.y;

			// Set shader properties
			var pss = panel.GetComponentsInChildren<ParticleSystem>();
			foreach (var ps in pss)
			{
				var render = ps.GetComponent<Renderer>();
				if(render == null)
				{
					continue;
				}
				if(render.material != null)
				{
					//Shader.FindObjectOfType()
					if(render.material.shader.name.Equals(need_Change_Shader_Additive))
					{
						render.material.shader = shader_Additive_ClipByUI;
					}
					else if(render.material.shader.name.Equals(need_Change_Shader_Blend_Additive))
					{
						render.material.shader = shader_Additive_Blend_ClipByUI;
					}
					else
					{
						continue;
					}
					
					render.material.SetFloat(m_panelSizeXProperty, topRight.x - leftBottom.x);
					render.material.SetFloat(m_panelSizeYProperty, topRight.y - leftBottom.y);
					render.material.SetVector(m_panelCenterAndSharpnessProperty, panelCenterAndSharpness);
				}
			}
		}
	}

}
