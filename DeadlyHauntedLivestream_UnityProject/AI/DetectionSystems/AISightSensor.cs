using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI{

    [ExecuteInEditMode]
    public class AISightSensor : MonoBehaviour{

        [Header("Detection Settings")]
        [SerializeField] private float _range;
        [SerializeField] private float _angle;
        [SerializeField] private float height;
        [SerializeField] private Vector3 _initialPosition;
        [SerializeField] private int _scanFrequency = 30;
        [SerializeField] private int _maxDetectables = 50;
        [SerializeField] private LayerMask _detectableLayers;
        [SerializeField] private LayerMask _obstructableLayers;

        private const string ON_LOST_OBJECT = "OnLostObject";
        private const string ON_DETECTED_OBJECT = "OnDetectedObject";
        private const string TARGET = "Target";

        private Collider[] _colliders;
        private List<GameObject> _objectsOnSight = new List<GameObject>();
        private Vector3 _initialCenterPosition;
        private GameObject _objectDetected;
        private int _count;
        private float _scanInterval;
        private float _scanTimer;

        [Header("Debugging Visuals Settings")]
        [SerializeField] private bool _enableDebugVisuals = true;
        [SerializeField] private Color _debugMeshColor = Color.green;
        [SerializeField] private Color _debugColorObjectDectected = Color.green;
        [SerializeField] private float _debugSphereSizeDetectedObjects = 0.2f;
        [SerializeField] private int _debugSegments = 10;
        private Mesh _debugMesh;

        public float Range => _range;
        public float Angle => _angle;

        private void Start(){
            //Initializing variables.

            _initialCenterPosition = new Vector3(_initialPosition.x, _initialPosition.y + height/2, _initialPosition.z);
            _scanInterval = 1/_scanFrequency;
            _colliders = new Collider[_maxDetectables];
        }

        void Update(){
            // Timer for frequency of scanning
            _scanTimer -= Time.deltaTime;
            if (_scanTimer <= 0){
                _scanTimer += _scanInterval;
                Scan();
            }
        }

        private void Scan(){
            //Scan the view for objects and invoke events if so.

            _count = Physics.OverlapSphereNonAlloc(transform.position + _initialCenterPosition, _range, 
                                                _colliders, _detectableLayers, 
                                                QueryTriggerInteraction.Collide);
            
            bool anythingDetected = false;
            bool sameTarget = false;
            _objectsOnSight.Clear();

            for(int i = 0; i < _count; i++){

                if(IsInSight(_colliders[i].gameObject)){

                    _objectsOnSight.Add(_colliders[i].gameObject);
                    anythingDetected = true;

                    if(_colliders[i].gameObject == _objectDetected){
                        sameTarget = true;
                        break;
                    }
                }
            }

            if(anythingDetected){
                if(!sameTarget){
                    //Logic to find the closest one.
                    float shortest = (_objectsOnSight[0].transform.position - transform.position).magnitude;

                    for(int i = 0; i < _objectsOnSight.Count; i++){
                        float distance = (_objectsOnSight[i].transform.position - transform.position).magnitude;
                        if(distance <= shortest){
                            _objectDetected = _objectsOnSight[i];
                            shortest = distance;
                        }
                    }

                    //Call Event of object detected.
                    ObjectDetected passObjectDetected = new ObjectDetected(_objectDetected, 0, 0);
                    ObjectMessenger.SetGameObject(TARGET, passObjectDetected.Instigator);
                    Debug.Log("Player in Sight");
                    EventMessenger.TriggerEvent(ON_DETECTED_OBJECT);
                }
            }
            else{
                if(_objectDetected != null){
                    _objectDetected = null;
                    EventMessenger.TriggerEvent(ON_LOST_OBJECT);
                }
            }
        }

        private bool IsInSight(GameObject target){
            // Check if the object is within angle of view, height and if it's not being blocked by something.

            bool check = false;

            Vector3 targetDirection = target.transform.position - transform.position;
            Quaternion originToTargetRotation = Quaternion.FromToRotation(transform.forward, targetDirection);
            
            if((_angle >= originToTargetRotation.eulerAngles.y) || (360-_angle <= originToTargetRotation.eulerAngles.y)){
                // If the object is within view angle.

                if(_initialPosition.y <= target.transform.position.y && target.transform.position.y <= _initialPosition.y + height){
                    // If object is within height if view check is there is an object in the way.

                    check = !Physics.Raycast(transform.position, targetDirection, targetDirection.magnitude, _obstructableLayers);
                }
            }

            return  check;
        }

        //__________________________________________________DEBUGGING VISUALS_____________________________________________________________

        private Mesh CreateDebugWedge(){
            // This creates a visual to aid debugging.

            Mesh mesh = new Mesh();

            int numberOfTriangles = (_debugSegments * 4) + 2 + 2;
            int numberVertices = numberOfTriangles * 3;

            Vector3[] vertices = new Vector3[numberVertices];
            int[] triangles = new int[numberVertices];

            Vector3 bottomCenter = Vector3.zero + _initialPosition;
            Vector3 bottomLeft = Quaternion.Euler(0, -_angle, 0) * Vector3.forward * _range + _initialPosition;
            Vector3 bottomRight = Quaternion.Euler(0, _angle, 0) * Vector3.forward * _range + _initialPosition;

            Vector3 topCenter = bottomCenter + Vector3.up * height;
            Vector3 topRight = bottomRight + Vector3.up * height;
            Vector3 topLeft = bottomLeft + Vector3.up * height;

            int vert = 0;

            // left side
            vertices[vert++] = bottomCenter;
            vertices[vert++] = bottomLeft;
            vertices[vert++] = topLeft;

            vertices[vert++] = topLeft;
            vertices[vert++] = topCenter;
            vertices[vert++] = bottomCenter;

            // right side
            vertices[vert++] = bottomCenter;
            vertices[vert++] = topCenter;
            vertices[vert++] = topRight;

            vertices[vert++] = topRight;
            vertices[vert++] = bottomRight;
            vertices[vert++] = bottomCenter;

            float currentAngle = -_angle;
            float deltaAngle = (_angle*2)/_debugSegments;
            
            for(int i = 0; i < _debugSegments; ++i){

                bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * _range + _initialPosition;
                bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * _range + _initialPosition;

                topRight = bottomRight + Vector3.up * height;
                topLeft = bottomLeft + Vector3.up * height;

                // far side
                vertices[vert++] = bottomLeft;
                vertices[vert++] = bottomRight;
                vertices[vert++] = topRight;

                vertices[vert++] = topRight;
                vertices[vert++] = topLeft;
                vertices[vert++] = bottomLeft;

                // top
                vertices[vert++] = topCenter;
                vertices[vert++] = topLeft;
                vertices[vert++] = topRight;

                // bottom
                vertices[vert++] = bottomCenter;
                vertices[vert++] = bottomRight;
                vertices[vert++] = bottomLeft;

                currentAngle += deltaAngle;
            }

            for(int i = 0; i < numberVertices; ++i){
                triangles[i] = i;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        private void OnValidate(){
            _debugMesh = CreateDebugWedge();
            _scanInterval = 1/_scanFrequency;
        }

        private void OnDrawGizmos(){
            if(_enableDebugVisuals){
                if(_debugMesh){
                    Gizmos.color = _debugMeshColor;
                    Gizmos.DrawMesh(_debugMesh, transform.position, transform.rotation);
                }

                Gizmos.DrawWireSphere(transform.position + _initialCenterPosition, _range);

                for(int i = 0; i < _count; ++i){

                    Gizmos.color = _debugColorObjectDectected;

                    if(_objectDetected != null){
                        Gizmos.DrawSphere(_objectDetected.transform.position, 
                                    _objectDetected.transform.localScale.magnitude * _debugSphereSizeDetectedObjects);
                        break;
                    }
                }
            }
        }
    }
}

