using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class TilemapShadows : MonoBehaviour
{
    CompositeCollider2D tilemapCollider;
    EdgeCollider2D edgeCollider;
    public bool selfShadows = false;
    public void Start()
    {
        tilemapCollider = GetComponent<CompositeCollider2D>();
        GameObject shadowCasterContainer = GameObject.Find("shadow_casters");
        for (int i = 0; i < tilemapCollider.pathCount; i++)
        {
            Vector2[] pathVertices = new Vector2[tilemapCollider.GetPathPointCount(i)];
            tilemapCollider.GetPath(i, pathVertices);
            GameObject shadowCaster = new GameObject("shadow_caster_" + i);
            PolygonCollider2D shadowPolygon = (PolygonCollider2D)shadowCaster.AddComponent(typeof(PolygonCollider2D));
            shadowCaster.transform.parent = shadowCasterContainer.transform;
            shadowCaster.transform.position = transform.position;
            shadowCaster.transform.rotation = transform.rotation;
            shadowPolygon.points = pathVertices;
            shadowPolygon.enabled = false;
            ShadowCaster2D shadowCasterComponent = shadowCaster.AddComponent<ShadowCaster2D>();
            shadowCasterComponent.selfShadows = selfShadows;

            // need to do "reflection" here, supposedly not the most efficient way, but it works.
            // fix so the shadows don't go through walls
            var fieldInfo = typeof(ShadowCaster2D).GetField("m_ApplyToSortingLayers", BindingFlags.Instance | BindingFlags.NonPublic);
            int id = SortingLayer.NameToID("Default");
            fieldInfo.SetValue(shadowCasterComponent, new[] { id });
        }
    }
}
