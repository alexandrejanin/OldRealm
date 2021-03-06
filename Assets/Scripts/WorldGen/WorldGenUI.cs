using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WorldGenUI : MonoBehaviour {
	[SerializeField] private Text tileInfo, mapInfo;

	private Map map;
	private MapDisplay mapDisplay;
	private WorldGenUtility worldGenUtility;

	private Tile tile;

	public static MapDrawMode DrawMode { get; private set; }
	private int mapDrawModesCount;

	private new Camera camera;

	private void Awake() {
		camera = Camera.main;
		mapDisplay = GetComponent<MapDisplay>();
		worldGenUtility = GetComponent<WorldGenUtility>();
		mapDrawModesCount = Enum.GetValues(typeof(MapDrawMode)).Length;
	}

	public void OnDrawModeChanged(int value) {
		DrawMode = value < mapDrawModesCount ? (MapDrawMode) value : 0;
		mapDisplay.DrawTexture();
	}

	public void OnMapChanged() {
		map = GameController.Map;

		string mapText = $"Seed: {map.settings.seed}\nPopulation: {map.towns.Sum(t => t.population)}";

		foreach (Climate climate in GameController.Climates) {
			List<Region> validRegions = map.regions.Where(region => region.climate == climate).ToList();
			int regionsCount = validRegions.Count;
			if (regionsCount == 0) continue;
			int tilesCount = validRegions.Sum(region => region.Size);

			mapText += $"\n{regionsCount} {climate.name}s ({tilesCount} tiles)";
		}

		if (mapInfo != null) mapInfo.text = mapText;
	}

	private void Update() {
		if (GameController.Location != null || map == null || GameController.WorldCamera.dragged) return;

		RaycastHit hit;
		if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit)) {
			int x = Mathf.FloorToInt(hit.point.x);
			int y = GameController.Map.size - Mathf.CeilToInt(hit.point.z) - 1;

			Tile newTile = map.GetTile(x, y);

			if (newTile == null) return;

			if (newTile != tile) {
				tile = newTile;
				string text = $"x: {x} y: {y}" +
				              $"\nHeight: {worldGenUtility.WorldHeightToMeters(tile.height)}m ({tile.height:F2})" +
				              $"\nTemp: {worldGenUtility.TemperatureToCelsius(tile.temp)}°C ({tile.temp:F2})" +
				              $"\nRegion: {tile.region}";
				if (tile.location != null) text += $"\n{tile.location}";
				Town town = tile.location as Town;
				if (town != null) text += $"\nPopulation: {town.population}";

				tileInfo.text = text;
			}

			if (Input.GetMouseButtonDown(0) && tile.location != null) {
				StartCoroutine(GameController.LoadLocation(tile.location));
			}

			if (Input.GetMouseButtonDown(1)) {
				Vector3 point = hit.point;
				point.y = GameController.WorldCamera.targetPos.y;
				GameController.WorldCamera.targetPos = point;
			}
		}
	}
}