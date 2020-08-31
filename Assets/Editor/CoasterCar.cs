using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ParkitectAssetEditor
{
    public class CoasterCar
    {
        public string Guid { get; private set; }

		[JsonIgnore]
		public GameObject GameObject
		{
			get { return GameObjectHashMap.Instance.GetGameObject(Guid) as GameObject;; }
			set { GameObjectHashMap.Instance.SetGameObject(Guid, value); }
		}

        public float SeatWaypointOffset = 0.2f;
        public float OffsetBack;
        public float OffsetFront;
		public List<CoasterRestraints> Restraints = new List<CoasterRestraints>();

		public CoasterCar(string guid)
		{
			Guid = guid;
		}
	}
}
