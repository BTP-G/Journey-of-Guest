using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JoG.Character.Move {

    public interface ICharacterController {

        void Enable();

        void Disable();

        void UpdateMoveDirection(in Vector3 moveDirection);

        void UpdateAimDirection(in Vector3 aimDirection);
    }
}