using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI{

    public class TaskBase : MonoBehaviour{

        protected AIController _aiController;

        protected const string EM_AIC_ON_TASK_DONE = "OnAITaskDone";
        
        private void Start(){
            _aiController = GetComponent<AIController>();
        }

        public void SetAIContorller(AIController newAIController){
            _aiController = newAIController;
        }

        public virtual void Enable(){}

        public virtual void Disable(){}
    }
}
