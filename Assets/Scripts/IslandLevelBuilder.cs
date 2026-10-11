using System.Collections.Generic;
using UnityEngine;

namespace CoinQuest
{
    /// <summary>Builds the complete playable island from Unity primitives at runtime.</summary>
    public sealed class IslandLevelBuilder : MonoBehaviour
    {
        public Camera GameCamera { get; private set; }
        private Transform worldRoot;
        private readonly List<Material> runtimeMaterials = new List<Material>();
        private readonly Dictionary<Color, Material> colorMaterials = new Dictionary<Color, Material>();
        private GameManager manager;
        private bool femaleExplorerSelected;
        private Transform maleExplorerVisual;
        private Transform femaleExplorerVisual;

        public bool IsFemaleExplorerSelected { get { return femaleExplorerSelected; } }

        public void SelectExplorer(bool female)
        {
            femaleExplorerSelected = female;
            if (maleExplorerVisual != null) maleExplorerVisual.gameObject.SetActive(!female);
            if (femaleExplorerVisual != null) femaleExplorerVisual.gameObject.SetActive(female);
        }

        public Material GlowMaterial(Color color)
        {
            var material = new Material(Shader.Find("Standard"));
            material.color = color;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.8f);
            runtimeMaterials.Add(material);
            return material;
        }

        public void BuildWorld(GameManager game) { manager = game; CreateWorld(); }
        public void RebuildWorld(GameManager game)
        {
            manager = game;
            if (worldRoot != null) Destroy(worldRoot.gameObject);
            CreateWorld();
        }

        private void CreateWorld()
        {
            worldRoot = new GameObject("Lost Temple Island").transform;
            worldRoot.SetParent(transform, false);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.36f, 0.55f, 0.67f);
            RenderSettings.ambientEquatorColor = new Color(0.28f, 0.35f, 0.28f);
            RenderSettings.ambientGroundColor = new Color(0.12f, 0.16f, 0.11f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(0.36f, 0.62f, 0.7f); RenderSettings.fogDensity = 0.006f;
            var lightObject = new GameObject("Island Sun"); lightObject.transform.SetParent(worldRoot, false); lightObject.transform.rotation = Quaternion.Euler(48, -32, 0);
            var sun = lightObject.AddComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.25f; sun.color = new Color(1f, 0.88f, 0.68f); sun.shadows = LightShadows.Soft;
            CreatePrimitive("Ocean", PrimitiveType.Cube, new Vector3(0,-3.8f,7), new Vector3(180,2,180), new Color(0.035f,0.27f,0.38f), false);
            CreatePrimitive("Main Island", PrimitiveType.Cylinder, new Vector3(0,-2.2f,7), new Vector3(48,2.2f,50), new Color(0.34f,0.48f,0.28f), false);
            CreatePrimitive("Island Top", PrimitiveType.Cylinder, new Vector3(0,-0.3f,7), new Vector3(44,0.26f,46), new Color(0.48f,0.54f,0.32f), false);
            BuildPath(); BuildLongRoute(); BuildScenery(); BuildObjectives(); BuildTemple(); BuildEnemies(); BuildFinalGuardian(); BuildPlayer();
        }

        private void BuildPath()
        {

            Vector3[] points = { new Vector3(0,0,0), new Vector3(0,0,7), new Vector3(3,0,13), new Vector3(0,0,19), new Vector3(0,0,26) };
            for (int i=0; i<points.Length-1; i++)
            {
                Vector3 mid = (points[i]+points[i+1])*0.5f; Vector3 d=points[i+1]-points[i];
                var path=CreatePrimitive("Ancient Path", PrimitiveType.Cube, mid+Vector3.down*(0.01f+i*0.003f), new Vector3(6,0.2f,d.magnitude+0.05f), new Color(0.43f,0.39f,0.31f), true);
                path.transform.rotation=Quaternion.LookRotation(d);
                for (int j=0;j<5;j++) { Vector3 p=Vector3.Lerp(points[i],points[i+1],(j+0.5f)/5f); CreatePrimitive("Path Stone",PrimitiveType.Cube,p+new Vector3((j%2==0?1:-1)*2.4f,0.13f,0),new Vector3(0.6f,0.24f,0.7f),new Color(0.53f,0.5f,0.4f),false); }
            }
        }

        private const float FinalArenaZ = 300f;
        private void BuildLongRoute()
        {
            Color[] areaColors = { new Color(0.35f,0.31f,0.23f), new Color(0.29f,0.36f,0.24f), new Color(0.42f,0.39f,0.31f), new Color(0.34f,0.36f,0.45f), new Color(0.42f,0.34f,0.25f) };
            Color[] areas = { new Color(0.24f,0.42f,0.23f), new Color(0.15f,0.32f,0.19f), new Color(0.42f,0.39f,0.31f), new Color(0.34f,0.45f,0.58f), new Color(0.48f,0.42f,0.31f) };
            for (int start = 28; start < FinalArenaZ; start += 24)
            {
                int zone = Mathf.Min(4, start / 60);
                float length = Mathf.Min(24f, FinalArenaZ - start);
                CreatePrimitive("Adventure Route - Zone " + (zone + 1), PrimitiveType.Cube,
                    new Vector3(0f, -0.02f, start + length * 0.5f), new Vector3(12f, 0.28f, length), areaColors[zone], true);

                // Coin lines and simple readable obstacles establish a repeating runner rhythm.
                if (start % 48 == 28)
                {
                    int lane = (start / 48) % 3 - 1;
                    for (int i = 0; i < 3; i++) CreateTrackCoin(new Vector3(lane * 3f, 0.95f, start + 5f + i * 2.3f));
                }
                if (start > 80 && start % 120 == 28)
                {
                    int lane = ((start / 120) % 3 - 1);
                    var obstacle = CreatePrimitive("Runner Fallen Log", PrimitiveType.Cylinder,
                        new Vector3(lane * 3f, 0.55f, start + 15f), new Vector3(0.55f, 2.4f, 0.55f), new Color(0.28f,0.16f,0.08f), true);
                    obstacle.transform.rotation = Quaternion.Euler(0, 0, 90);
                    obstacle.GetComponent<Collider>().isTrigger = true;
                    obstacle.AddComponent<Hazard>();
                }
                if (start % 144 == 28)
                {
                    for (int side = -1; side <= 1; side += 2)
                    {
                        float z = start + 8f;
                        CreatePrimitive(zone < 2 ? "Jungle Palm" : zone == 3 ? "Sky Ruin Pillar" : "Temple Trail Pillar",
                            zone < 2 ? PrimitiveType.Cylinder : PrimitiveType.Cube,
                            new Vector3(side * 9f, zone == 3 ? 2.1f : 1.8f, z),
                            zone < 2 ? new Vector3(0.55f,1.8f,0.55f) : new Vector3(1.1f,3.6f,1.1f), areas[zone], false);
                    }
                }
            }

            // Six unmistakable landmarks divide the long, connected route into readable regions.
            for (int area = 1; area <= 4; area++)
            {
                float z = area * 60f;
                Color glow = area == 3 ? new Color(0.25f,0.75f,1f) : new Color(1f,0.68f,0.2f);
                for (int side = -1; side <= 1; side += 2)
                    CreatePrimitive("Zone Gate Pillar", PrimitiveType.Cube, new Vector3(side * 5.4f, 2.5f, z), new Vector3(0.8f,5f,0.8f), areas[(area - 1) % 5], true);
                var lintel = CreatePrimitive("Zone Gate Glow", PrimitiveType.Cube, new Vector3(0f,5f,z), new Vector3(11.5f,0.45f,0.7f), glow, false);
                lintel.GetComponent<Renderer>().material = GlowMaterial(glow);
            }

            // The flight reward and elevated coin arc sit over an uninterrupted safe ground lane.
            CreateWingPickup(new Vector3(0f, 1.55f, 75f));
            for (int i = 0; i < 9; i++) CreateTrackCoin(new Vector3((i % 3 - 1) * 2.3f, 2.4f + (i % 3) * 1.1f, 85f + i * 5f));
            CreateHealingPickup(new Vector3(0f, 0.55f, 55f));
            CreateHealingPickup(new Vector3(0f, 0.55f, 140f));
            CreateHealingPickup(new Vector3(0f, 0.55f, 230f));
            CreateHealingPickup(new Vector3(0f, 0.55f, 270f));
            
            CreateCheckpoint(new Vector3(0f, 0.18f, 65f));
            CreateCheckpoint(new Vector3(0f, 0.18f, 150f));
            CreateCheckpoint(new Vector3(0f, 0.18f, 220f));
            
            CreateRune(0, "Jungle", new Vector3(0f,1.25f,40f));
            CreateRune(1, "Sky", new Vector3(0f,1.25f,125f));
            CreateRune(2, "Temple", new Vector3(0f,1.25f,200f));
        }

        private void CreateTrackCoin(Vector3 position)
        {
            var coin = CreatePrimitive("Golden Coin", PrimitiveType.Cylinder, position, new Vector3(0.65f,0.12f,0.65f), new Color(1f,0.72f,0.12f), true);
            coin.transform.rotation = Quaternion.Euler(90f,0f,0f);
            coin.GetComponent<Collider>().isTrigger = true;
            coin.AddComponent<CoinCollectible>();
        }
        private void CreateWingPickup(Vector3 position)
        {
            var wing = new GameObject("Golden Wings"); wing.transform.SetParent(worldRoot,false); wing.transform.position=position;
            var trigger=wing.AddComponent<SphereCollider>(); trigger.radius=1.1f; trigger.isTrigger=true;
            var gold=GlowMaterial(new Color(1f,0.65f,0.12f));
            var left=CreateChildPrimitive(wing.transform,"Left Golden Wing",PrimitiveType.Sphere,new Vector3(-0.63f,0f,0f),new Vector3(1.15f,0.48f,0.22f),new Color(1f,0.72f,0.12f),false);
            var right=CreateChildPrimitive(wing.transform,"Right Golden Wing",PrimitiveType.Sphere,new Vector3(0.63f,0f,0f),new Vector3(1.15f,0.48f,0.22f),new Color(1f,0.72f,0.12f),false);
            left.GetComponent<Renderer>().sharedMaterial=gold; right.GetComponent<Renderer>().sharedMaterial=gold;
            var core=CreateChildPrimitive(wing.transform,"Wing Core",PrimitiveType.Sphere,Vector3.zero,new Vector3(0.38f,0.5f,0.26f),Color.white,false);
            core.GetComponent<Renderer>().sharedMaterial=gold;
            wing.AddComponent<WingPickup>();
        }
        private void CreateHealingPickup(Vector3 position)
        {
            var fruit = CreatePrimitive("Healing Fruit", PrimitiveType.Sphere, position, Vector3.one * 0.8f, new Color(0.2f,0.9f,0.35f), true);
            fruit.GetComponent<Renderer>().material = GlowMaterial(new Color(0.2f,1f,0.35f));
            fruit.GetComponent<Collider>().isTrigger = true;
            var body = fruit.AddComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
            fruit.AddComponent<HealingPickup>();
        }
        private void CreateCheckpoint(Vector3 position)
        {
            var cp = CreatePrimitive("Adventure Checkpoint", PrimitiveType.Cylinder, position, new Vector3(2.5f,0.18f,2.5f), new Color(0.15f,0.65f,0.6f), true);
            cp.GetComponent<Collider>().isTrigger = true;
            cp.AddComponent<Checkpoint>();
        }
        private void CreateRune(int id, string label, Vector3 position)
        {
            Color color = id == 0 ? new Color(0.15f,0.82f,1f) : id == 1 ? new Color(0.72f,0.3f,1f) : new Color(0.2f,1f,0.57f);
            var rune = CreatePrimitive(label + " Rune", PrimitiveType.Cube, position, new Vector3(0.75f,1.35f,0.35f), color, true);
            rune.GetComponent<Renderer>().material = GlowMaterial(color);
            rune.GetComponent<Collider>().isTrigger = true;
            rune.AddComponent<RuneCollectible>().Configure(id,label);
        }

        private void BuildTemple()
        {
            Color stone = new Color(0.49f,0.48f,0.4f);
            float doorZ = FinalArenaZ;
            CreatePrimitive("Final Temple Foundation",PrimitiveType.Cube,new Vector3(0,-0.02f,doorZ+9),new Vector3(20,0.24f,20),stone,true);
            for(int side=-1;side<=1;side+=2)
            {
                CreatePrimitive("Final Temple Pillar",PrimitiveType.Cube,new Vector3(side*6,3.3f,doorZ-2),new Vector3(1.6f,6,1.7f),stone,true);
                CreatePrimitive("Final Temple Pillar",PrimitiveType.Cube,new Vector3(side*6,3.3f,doorZ+8),new Vector3(1.6f,6,1.7f),stone,true);
                CreatePrimitive("Final Temple Wing",PrimitiveType.Cube,new Vector3(side*8.2f,3.5f,doorZ+4),new Vector3(1.2f,6.4f,12),stone,true);
            }
            CreatePrimitive("Final Temple Lintel",PrimitiveType.Cube,new Vector3(0,6.25f,doorZ-2),new Vector3(13,1.1f,1.7f),stone,true);
            var door=CreatePrimitive("Sealed Temple Door",PrimitiveType.Cube,new Vector3(0,2.9f,doorZ-1.7f),new Vector3(4.7f,5.1f,0.8f),new Color(0.21f,0.16f,0.11f),true);
            door.AddComponent<TempleDoor>().Configure(door.transform, door.GetComponent<Collider>());
            for(int i=0;i<5;i++) CreatePrimitive("Temple Step",PrimitiveType.Cube,new Vector3(0,0.3f+i*0.22f,doorZ-7+i*0.75f),new Vector3(8-i*0.55f,0.3f,1.1f),new Color(0.47f,0.45f,0.37f),true);
            var chest=CreatePrimitive("Ancient Relic Chest",PrimitiveType.Cube,new Vector3(0,1.45f,doorZ+11),new Vector3(2.5f,1.7f,1.6f),new Color(0.35f,0.19f,0.07f),true);
            CreatePrimitive("Chest Gold Trim",PrimitiveType.Cube,new Vector3(0,1.5f,doorZ+10.17f),new Vector3(2.55f,0.18f,0.08f),new Color(1f,0.66f,0.1f),false);
            var chestLid=CreatePrimitive("Chest Lid",PrimitiveType.Cube,new Vector3(0,2.35f,doorZ+11),new Vector3(2.55f,0.3f,1.65f),new Color(0.45f,0.25f,0.08f),false);
            chest.AddComponent<TreasureChest>().Configure(chestLid.transform);
            CreatePrimitive("Relic Glow",PrimitiveType.Sphere,new Vector3(0,2.8f,doorZ+11),Vector3.one*0.55f,new Color(1f,0.72f,0.12f),false).GetComponent<Renderer>().material=GlowMaterial(new Color(1f,0.62f,0.08f));
            CreatePrimitive("Boss Arena Floor",PrimitiveType.Cube,new Vector3(0,-0.12f,doorZ-55),new Vector3(30,0.3f,34),new Color(0.35f,0.34f,0.31f),true);
            for(int side=-1;side<=1;side+=2) CreatePrimitive("Boss Arena Wall",PrimitiveType.Cube,new Vector3(side*14,2f,doorZ-55),new Vector3(1f,4f,34f),stone,true);
            CreatePrimitive("Boss Arena Entry Arch",PrimitiveType.Cube,new Vector3(0,5f,doorZ-38),new Vector3(12f,0.6f,0.8f),stone,true);
        }

        private void BuildObjectives()
        {
            var hazard=CreatePrimitive("Jungle Thorn Patch",PrimitiveType.Cube,new Vector3(2.6f,0.12f,19),new Vector3(3.2f,0.2f,2.5f),new Color(0.45f,0.19f,0.14f),true);
            hazard.GetComponent<BoxCollider>().isTrigger=true; hazard.AddComponent<Hazard>();
            for(int i=0;i<5;i++) CreateTrackCoin(new Vector3((i%3-1)*2f,0.9f,5f+i*3.8f));
            var hidden=CreatePrimitive("Echo Bridge",PrimitiveType.Cube,new Vector3(-5,0.2f,23),new Vector3(7,0.45f,2.5f),new Color(0.17f,0.77f,0.92f),true);
            hidden.GetComponent<Renderer>().material=GlowMaterial(new Color(0.12f,0.72f,1f)); hidden.AddComponent<EchoReveal>();
        }

        private void BuildEnemies()
        {
            float[] guardians = { 28f, 52f, 105f, 145f, 180f, 220f };
            for (int i = 0; i < guardians.Length; i++)
                CreateEnemyVisual("Stone Guardian", new Vector3(i % 2 == 0 ? -3f : 3f,0f,guardians[i]), 3 + i % 3, false, 1.8f);
            CreateEnemyVisual("Shadow Bat", new Vector3(-2f,3f,100f), 3, true, 2.6f);
            CreateEnemyVisual("Shadow Bat", new Vector3(2f,4f,185f), 3, true, 2.6f);
        }

        private void BuildFinalGuardian()
        {
            Vector3 position = new Vector3(0f,0.18f,FinalArenaZ-55f);
            var root = new GameObject("Ancient Sky Guardian Boss"); root.transform.SetParent(worldRoot,false); root.transform.position=position; root.transform.localScale=Vector3.one*1.6f;
            var collider=root.AddComponent<CapsuleCollider>(); collider.height=3.4f; collider.radius=1.1f; collider.center=new Vector3(0,2.3f,0);
            Transform leftArm,rightArm,leftLeg,rightLeg;
            CharacterModelFactory.CreateGuardian(root.transform,out leftArm,out rightArm,out leftLeg,out rightLeg);
            var enemy=root.AddComponent<EnemyController>(); enemy.Configure(8,false,28f,1.7f); enemy.SetFinalBoss(true);
            manager.RegisterFinalBoss(enemy);
        }
        private void CreateEnemyVisual(string enemyName, Vector3 position, int hitPoints, bool flying, float speed)
        {
            if (!flying) position.y = 0.12f;
            var root = new GameObject(enemyName); root.transform.SetParent(worldRoot, false); root.transform.position = position;
            var collider = root.AddComponent<CapsuleCollider>();
            collider.height = flying ? 1.25f : 3f;
            collider.radius = flying ? 0.38f : 0.78f;
            collider.center = flying ? new Vector3(0, 0.25f, 0) : new Vector3(0, 1.45f, 0);
            if (flying)
            {
                Transform leftWing, rightWing;
                CharacterModelFactory.CreateShadowBat(root.transform, out leftWing, out rightWing);
            }
            else
            {
                Transform leftArm, rightArm, leftLeg, rightLeg;
                CharacterModelFactory.CreateGuardian(root.transform, out leftArm, out rightArm, out leftLeg, out rightLeg);
            }
            var enemy = root.AddComponent<EnemyController>(); enemy.Configure(hitPoints, flying, 13, speed);
        }

        private Vector3 FindSafeSpawnPosition(Vector3 requested)
        {
            Physics.SyncTransforms();
            Vector3 rayStart = requested + Vector3.up * 25f;
            RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, 50f, ~0, QueryTriggerInteraction.Ignore);
            float groundY = float.NegativeInfinity;
            foreach (RaycastHit hit in hits)
            {
                if (hit.normal.y < 0.65f || hit.point.y > requested.y + 2f) continue;
                groundY = Mathf.Max(groundY, hit.point.y);
            }
            if (float.IsNegativeInfinity(groundY)) Debug.LogError("Coin Quest spawn has no walkable ground collider beneath it.");
            requested.y = float.IsNegativeInfinity(groundY) ? 0f : groundY;
            return requested;
        }

        private void BuildPlayer()
        {
            var player=new GameObject("Adventurer"); player.transform.SetParent(worldRoot,false); player.transform.position=FindSafeSpawnPosition(new Vector3(0f, 0f, 0.5f));
            var motor=player.AddComponent<CharacterController>(); motor.height=2f; motor.radius=0.42f; motor.center=new Vector3(0,1f,0); motor.slopeLimit=48; motor.stepOffset=0.35f;
            var visualRoot = new GameObject("Explorer Character Selection").transform;
            visualRoot.SetParent(player.transform, false);
            maleExplorerVisual = CharacterModelFactory.CreateExplorer(visualRoot, false);
            femaleExplorerVisual = CharacterModelFactory.CreateExplorer(visualRoot, true);
            SelectExplorer(femaleExplorerSelected);
            var camObj=new GameObject("Coin Quest Camera"); camObj.transform.SetParent(worldRoot,false); GameCamera=camObj.AddComponent<Camera>(); GameCamera.tag="MainCamera"; GameCamera.fieldOfView=62; GameCamera.nearClipPlane=0.1f; GameCamera.farClipPlane=250; camObj.AddComponent<AudioListener>();
            var rig=camObj.AddComponent<ThirdPersonCamera>(); rig.SetTarget(player.transform);
            Physics.SyncTransforms();
            CollisionFlags spawnContact = motor.Move(Vector3.down * 0.5f);
            if ((spawnContact & CollisionFlags.Below) == 0 && !motor.isGrounded)
                Debug.LogError("Adventurer did not contact the spawn floor. Check the entrance pad collider and CharacterController.");
            var controller=player.AddComponent<PlayerController>(); controller.Configure(rig,visualRoot);
            var health=player.AddComponent<PlayerHealth>(); player.AddComponent<PlayerInteractor>(); var pulse=player.AddComponent<EchoPulse>();
            var combat=player.AddComponent<PlayerCombat>(); combat.Configure(controller,pulse,GameCamera);
            manager.SetWorldReferences(player.transform,controller,health,worldRoot.GetComponentInChildren<TempleDoor>());
        }

        private GameObject CreateChildPrimitive(Transform parent,string name,PrimitiveType type,Vector3 position,Vector3 scale,Color color,bool solid)
        {
            var o=GameObject.CreatePrimitive(type); o.name=name; o.transform.SetParent(parent,false); o.transform.localPosition=position; o.transform.localScale=scale; o.GetComponent<Renderer>().material=Material(color);
            if(!solid) { var c=o.GetComponent<Collider>(); if(c!=null) Destroy(c); } return o;
        }
        private GameObject CreatePrimitive(string name,PrimitiveType type,Vector3 position,Vector3 scale,Color color,bool solid)
        {
            var o=GameObject.CreatePrimitive(type); o.name=name; o.transform.SetParent(worldRoot,false); o.transform.position=position; o.transform.localScale=scale; o.GetComponent<Renderer>().material=Material(color);
            if(!solid) { var c=o.GetComponent<Collider>(); if(c!=null) c.enabled=false; } return o;
        }
        private Material Material(Color color)
        {
            if (colorMaterials.TryGetValue(color, out Material cached)) return cached;
            var material = new Material(Shader.Find("Standard")); material.color = color;
            colorMaterials.Add(color, material); runtimeMaterials.Add(material); return material;
        }
        private void BuildScenery()
        {
            Random.InitState(2019);
            int placedPalms = 0;
            int attempts = 0;
            while (placedPalms < 34 && attempts < 500)
            {
                attempts++;
                float a=Random.Range(0,Mathf.PI*2),r=Random.Range(18f,23f); Vector3 p=new Vector3(Mathf.Cos(a)*r,0,7+Mathf.Sin(a)*r);
                // Keep trunks and wide fronds outside the starting camera view and the full walkable route.
                if (Vector3.Distance(p, new Vector3(0, 0, 0.5f)) < 12f || IsNearPlayablePath(p, 7f)) continue;
                var trunk=CreatePrimitive("Palm Trunk",PrimitiveType.Cylinder,p+Vector3.up*2.2f,new Vector3(0.48f,2.2f,0.48f),new Color(0.34f,0.22f,0.12f),true);
                trunk.transform.rotation=Quaternion.Euler(Random.Range(-9,9),0,Random.Range(-12,12));
                for(int j=0;j<5;j++) { var leaf=CreatePrimitive("Palm Frond",PrimitiveType.Sphere,p+Vector3.up*4.25f+Quaternion.Euler(0,j*72,0)*Vector3.forward*1.4f,new Vector3(0.55f,0.18f,2.2f),new Color(0.12f,0.39f,0.2f),false); leaf.transform.rotation=Quaternion.Euler(0,j*72,25); }
                placedPalms++;
            }
            for(int i=0;i<12;i++) { float x=Random.Range(-18,18),z=Random.Range(-9,23); Vector3 p=new Vector3(x,0.5f,z); if((Mathf.Abs(x)<5 && z<20) || Vector3.Distance(new Vector3(x,0,z),new Vector3(0,0,0.5f))<8f) continue; var rock=CreatePrimitive("Island Boulder",PrimitiveType.Sphere,p,new Vector3(Random.Range(1f,2.8f),Random.Range(0.7f,2f),Random.Range(1f,2.8f)),new Color(0.39f,0.4f,0.34f),true); rock.transform.rotation=Random.rotation; }
            CreatePrimitive("Distant Jungle Backdrop",PrimitiveType.Cylinder,new Vector3(0,11,360),new Vector3(46,13,18),new Color(0.12f,0.31f,0.24f),false);
        }

        private bool IsNearPlayablePath(Vector3 position, float clearance)
        {
            Vector3[] points = { new Vector3(0,0,0), new Vector3(0,0,7), new Vector3(3,0,13), new Vector3(0,0,19), new Vector3(0,0,26) };
            float clearanceSquared = clearance * clearance;
            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 segment = points[i + 1] - points[i];
                float t = Mathf.Clamp01(Vector3.Dot(position - points[i], segment) / segment.sqrMagnitude);
                Vector3 nearest = points[i] + segment * t;
                if ((position - nearest).sqrMagnitude < clearanceSquared) return true;
            }
            return false;
        }
    }
}

