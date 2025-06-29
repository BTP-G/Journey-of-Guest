using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace  JoG {
    public interface IEye {
        public Vector3 AimPosition { get; }
        public Vector3 AimOrigin { get; }
        public Vector3 AimVector3 { get; }
        public GameObject AimObject { get; }
    }
}
