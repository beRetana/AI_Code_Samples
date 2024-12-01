using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI{

    /* 
        Base class for any AI actor that will be created to ensure individual 
        behavior scripts have what they need to function.
    */
    
    public abstract class AIActor : MonoBehaviour{

        protected const string EM_AIC_ON_TASK_DONE = "OnAITaskDone";

        public abstract void TransitionOfBehaviors();

        public abstract void AbortBehaviors();

    }
}
