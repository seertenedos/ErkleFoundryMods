using UnityEngine;

namespace PlanIt
{

    public class RecipeRow : MonoBehaviour
    {
        [SerializeField] private Transform _outputs;
        [SerializeField] private Transform _machines;
        [SerializeField] private Transform _inputs;

        public Transform OutputsTransform => _outputs;
        public Transform MachinesTransform => _machines;
        public Transform InputsTransform => _inputs;
    }

}
