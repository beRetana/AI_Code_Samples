using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AI{

    public class AIController : MonoBehaviour{

        private enum State{
            Idle,
            Processing,
            Done
        }

        private enum TaskType{
            Movement,
        }

        private NavMeshAgent _agent;
        private State _state = State.Idle;
        private TaskType _taskType = TaskType.Movement;

        private bool _stateTriggered = true;
        private float _agentDefaultSpeed;

        private const string EM_AIC_TASK_DONE = "OnAITaskDone";

        public EventHandler taskCompleted;

        private void Start(){
            _agent = GetComponent<NavMeshAgent>();
            _agentDefaultSpeed = _agent.speed;
        }

        private void Update(){
            switch(_state){
                case State.Idle:
                    //Player is not moving
                    if(_stateTriggered){
                        Debug.Log($"State of task is {_state}");
                        _stateTriggered = false;
                    }
                    break;
                case State.Processing:

                    if(_stateTriggered){
                        Debug.Log($"State of task is {_state}");
                        _stateTriggered = false;
                    }

                    switch(_taskType){
                        case TaskType.Movement:
                            if (_agent.remainingDistance <= _agent.stoppingDistance){
                                _state = State.Done;
                                _stateTriggered = true;
                            }
                            break;
                    }
                    
                    break;
                case State.Done:
                    Debug.Log($"State of task is {_state}");
                    _state = State.Idle;
                    EventMessenger.TriggerEvent(EM_AIC_TASK_DONE);
                    taskCompleted?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        public void MoveTo(Vector3 targetLocation){
            _agent.SetDestination(targetLocation);
            _stateTriggered = true;
            _state = State.Processing;
        }

        public void MoveToRandomLocation(float radius){
            float xPos = UnityEngine.Random.Range(-radius,radius);
            float zPos = UnityEngine.Random.Range(-radius,radius);

            MoveTo(new Vector3(xPos, 0, zPos));
        }

        public void MoveAway(Vector3 awayLocation){

            MoveTo(-(awayLocation - transform.position));
        }

        public void ChangeSpeed(float newSpeed){
            _agent.speed = newSpeed;
        }

        public void DefaultSpeed(){
            _agent.speed = _agentDefaultSpeed;
        }

        public void AbortTask(){
            _state = State.Idle;
            _agent.SetDestination(transform.position);
            _stateTriggered = true;
            DefaultSpeed();
        }

        public bool HasReachedDestination()
        {
            if (_agent == null) return true;
            
            // Check if we're very close to the destination
            if (!_agent.pathPending)
            {
                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}
