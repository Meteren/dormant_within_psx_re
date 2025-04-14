using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombinable
{
    void OnTryCombine(ItemRepresenter representer);
}
