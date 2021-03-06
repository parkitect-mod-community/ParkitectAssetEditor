using System.Linq;
using Newtonsoft.Json;
using UnityEditor;

namespace ParkitectAssetEditor
{
    using System.Collections.Generic;
    using UnityEngine;

    public enum Temperature
    {
        NONE,
        COLD,
        HOT
    }

    public enum HandSide
    {
        LEFT,
        RIGHT
    }

    public enum ConsumeAnimation
    {
        GENERIC,
        DRINK_STRAW,
        LICK,
        WITH_HANDS
    }

    public enum ProductType
    {
        ON_GOING, // can't add an ongoing product
        CONSUMABLE,
        WEARABLE,
        BALLOON
    }

    public enum Seasonal
    {
        WINTER,
        SPRING,
        SUMMER,
        AUTUMN,
        NONE
    }

    public enum Body
    {
        HEAD,
        FACE,
        BACK
    }

    public enum EffectTypes
    {
        HUNGER,
        THIRST,
        HAPPINESS,
        TIREDNESS,
        SUGARBOOST
    }

    public class ShopProduct
    {
        public List<ShopIngredient> Ingredients = new List<ShopIngredient>();

        public ProductType ProductType { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether this instance has custom colors.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has custom colors; otherwise, <c>false</c>.
        /// </value>
        public bool HasCustomColors { get; set; }

        /// <summary>
        /// The colors
        /// </summary>
        [JsonIgnore] public Color[] Colors = new Color[4];

        /// <summary>
        /// Gets or sets the amount of custom colors this asset has.
        /// </summary>
        /// <value>
        /// The color count.
        /// </value>
        public int ColorCount { get; set; }

        /// <summary>
        /// Property to support serializing for Unity's color struct
        /// </summary>
        public CustomColor[] CustomColors
        {
            get
            {
                return Colors.Select(c => new CustomColor {Red = c.r, Green = c.g, Blue = c.b, Alpha = c.a}).ToArray();
            }
            set { Colors = value.Select(c => new Color(c.Red, c.Green, c.Blue, c.Alpha)).ToArray(); }
        }


        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public string Guid { get; set; }

        //trash
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public string TrashGuid { get; set; }


        /// <summary>
        /// Gets or sets the fence post GO.
        /// </summary>
        /// <value>
        /// The fence post GO.
        /// </value>
        [JsonIgnore]
        public GameObject Product
        {
            get { return GameObjectHashMap.Instance.GetGameObject(Guid); }
            set { GameObjectHashMap.Instance.SetGameObject(Guid, value); }
        }


        //base
        public string Name { get; set; }
        public float Price { get; set; }

        public bool IsTwoHanded { get; set; }
        public bool IsInterestingToLookAt { get; set; }
        public HandSide HandSide { get; set; }

        //ongoing
        public int Duration { get; set; }
        public bool RemoveWhenDepleted { get; set; }
        public bool DestroyWhenDepleted { get; set; }

        //wearable
        public Body BodyLocation { get; set; }
        public Seasonal SeasonalPreference { get; set; }
        public Temperature TemperaturePreference { get; set; }
        public bool HideOnRide { get; set; }
        public bool HideHair { get; set; }

        //consumable
        public ConsumeAnimation ConsumeAnimation { get; set; }
        public Temperature Temprature { get; set; }
        public int Portions { get; set; }

        //trash
        public float DisgustFactor = .5f;
        public float Volume = .1f;
        public bool CanWiggle = true;

        //balloon
        public float DefaultMass = 1.0f;
        public float DefaultDrag = 0.0f;
        public float DefaultAngularDrag = 0.05f;

        /// <summary>
        /// Gets or sets the fence post GO.
        /// </summary>
        /// <value>
        /// The fence post GO.
        /// </value>
        [JsonIgnore]
        public GameObject Trash
        {
            get { return GameObjectHashMap.Instance.GetGameObject(TrashGuid); }
            set { GameObjectHashMap.Instance.SetGameObject(TrashGuid, value); }
        }

        [JsonIgnore] private Vector2 _scrollPos;
        [JsonIgnore] private ShopIngredient _selected;


        public virtual void ShopProductSection()
        {
            Event e = Event.current;

            Name = EditorGUILayout.TextField("Name", Name);
            Price = EditorGUILayout.FloatField("Price ", Price);
            Product = EditorGUILayout.ObjectField("Product Object:", Product, typeof(GameObject), true) as GameObject;

            ProductType = (ProductType)( EditorGUILayout.Popup("Product Type", ((int)(ProductType - 1) < 0 ? 0 :  (int)(ProductType - 1)) , new []
            {
                ProductType.CONSUMABLE.ToString(),
                ProductType.WEARABLE.ToString(),
                ProductType.BALLOON.ToString()
            }) + 1);

            if (ProductType == ProductType.ON_GOING || ProductType == ProductType.CONSUMABLE)
            {
                HandSide = (HandSide) EditorGUILayout.EnumPopup("Hand Side", HandSide);
                IsTwoHanded = EditorGUILayout.Toggle("Is Two Handed", IsTwoHanded);
                IsInterestingToLookAt = EditorGUILayout.Toggle("Is Interesting To Look At", IsInterestingToLookAt);
            }

            switch (ProductType)
            {
                case ProductType.ON_GOING:
                {
                    Duration = EditorGUILayout.IntField("Duration ", Duration);
                    RemoveWhenDepleted = EditorGUILayout.Toggle("Remove When Depleted", RemoveWhenDepleted);
                    DestroyWhenDepleted = EditorGUILayout.Toggle("Destroy When Depleted", DestroyWhenDepleted);
                }
                    break;
                case ProductType.CONSUMABLE:
                {
                    ConsumeAnimation =
                        (ConsumeAnimation) EditorGUILayout.EnumPopup("Consume Animation ", ConsumeAnimation);
                    Temprature = (Temperature) EditorGUILayout.EnumPopup("Temperature ", Temprature);
                    Portions = EditorGUILayout.IntField("Portions ", Portions);

                    EditorGUILayout.LabelField("Trash:", EditorStyles.boldLabel);
                    Trash = EditorGUILayout.ObjectField("Trash Objects:", Trash, typeof(GameObject),
                        true) as GameObject;
                    if (Trash != null)
                    {
                        DisgustFactor = EditorGUILayout.Slider("Disgust", DisgustFactor, 0f, 1f);
                        Volume = EditorGUILayout.Slider("Volume", Volume, 0f, 1f);
                        CanWiggle = EditorGUILayout.Toggle("Can Wiggle", CanWiggle);
                    }
                }
                    break;
                case ProductType.WEARABLE:
                {
                    BodyLocation = (Body) EditorGUILayout.EnumPopup("Body Location ", BodyLocation);
                    SeasonalPreference =
                        (Seasonal) EditorGUILayout.EnumPopup("Seasonal Preference ", SeasonalPreference);
                    TemperaturePreference =
                        (Temperature) EditorGUILayout.EnumPopup("Temperature Preference", TemperaturePreference);
                    HideHair = EditorGUILayout.Toggle("Hide Hair", HideHair);
                    HideOnRide = EditorGUILayout.Toggle("Hide On Ride", HideOnRide);

                    HasCustomColors = EditorGUILayout.Toggle("Has custom colors: ", HasCustomColors);
                    if (HasCustomColors)
                    {
                        ColorCount = Mathf.RoundToInt(EditorGUILayout.Slider("Color Count: ", ColorCount, 1, 4));
                        for (int i = 0; i < ColorCount; i++)
                        {
                            Colors[i] = EditorGUILayout.ColorField("Color " + (i + 1), Colors[i]);
                        }
                    }
                }
                    break;
                case ProductType.BALLOON:
                {
                    DefaultMass = EditorGUILayout.FloatField("Default Mass", DefaultMass);
                    DefaultDrag = EditorGUILayout.FloatField("Default Drag", DefaultDrag);
                    DefaultAngularDrag = EditorGUILayout.FloatField("Default Angular Drag", DefaultAngularDrag);
                }
                    break;
            }


            EditorGUILayout.LabelField("Ingredients:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal(GUILayout.Height(300));
            EditorGUILayout.BeginVertical("ShurikenEffectBg", GUILayout.Width(150));
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(300));

            for (int i = 0; i < Ingredients.Count; i++)
            {
                Color gui = GUI.color;
                if (Ingredients[i] == _selected)
                {
                    GUI.color = Color.red;
                }

                if (GUILayout.Button(Ingredients[i].Name + "    $" + Ingredients[i].Price + ".00",
                    "ShurikenModuleTitle"))
                {

                    GUI.FocusControl("");
                    if (e.button == 1)
                    {
                        Ingredients.RemoveAt(i);
                        return;
                    }

                    if (_selected == Ingredients[i])
                    {
                        _selected = null;
                        return;
                    }

                    _selected = Ingredients[i];
                }

                GUI.color = gui;
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add Ingredients"))
            {
                Ingredients.Add(new ShopIngredient());
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            if (_selected != null)
            {
                if (!Ingredients.Contains(_selected))
                {
                    _selected = null;
                    return;
                }

                _selected.Name = EditorGUILayout.TextField("Ingredient Name ", _selected.Name);
                _selected.Price = EditorGUILayout.FloatField("Price ", _selected.Price);
                _selected.Amount = EditorGUILayout.FloatField("Amount (" + (int) (_selected.Amount * 100f) + ")",
                    _selected.Amount);
                _selected.Tweakable = EditorGUILayout.Toggle("Tweakable ", _selected.Tweakable);

                for (int i = 0; i < _selected.Effects.Count; i++)
                {
                    Color gui = GUI.color;

                    if (GUILayout.Button("Effector " + _selected.Effects[i].Type, "ShurikenModuleTitle"))
                    {

                        GUI.FocusControl("");
                        if (e.button == 1)
                        {
                            _selected.Effects.RemoveAt(i);
                            return;
                        }
                    }

                    _selected.Effects[i].Type =
                        (EffectTypes) EditorGUILayout.EnumPopup("Type ", _selected.Effects[i].Type);
                    _selected.Effects[i].Amount =
                        EditorGUILayout.Slider("Amount", _selected.Effects[i].Amount, 1f, -1f);
                    GUI.color = gui;
                }

                if (GUILayout.Button("Add Effect"))
                {
                    _selected.Effects.Add(new Effect());
                }

            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Asset"/> class.
        /// </summary>
        public ShopProduct()
        {
            Guid = GUID.Generate().ToString(); // don't need the object, just make it a string immediately
            TrashGuid = GUID.Generate().ToString();
        }

    }

    public class ShopIngredient
    {

        public string Name { get; set; }
        public float Price { get; set; }
        public float Amount { get; set; }
        public bool Tweakable { get; set; }
        public List<Effect> Effects = new List<Effect>();
    }


    public class Effect
    {
        public EffectTypes Type { get; set; }
        public float Amount { get; set; }
    }
}
