using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter{

    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player){
        if(!HasKitchenObject()){
            //there is no kitchen object here!
            if(player.HasKitchenObject()){
                //player is carrying smthing!
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
            else{
                //player has nothing!
            }
        }
        else{
            //there is a kitchen object here!
            if(player.HasKitchenObject()){
                //player is carrying smthing!
                if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)){
                    //player is carrying a plate!
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())){
                        GetKitchenObject().DestroySelf();
                    }
                }
                else{
                    //player is not carrying a plate!
                    if(GetKitchenObject().TryGetPlate(out plateKitchenObject)){
                        //the object on the counter is a plate!
                        if(plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO())){
                            player.GetKitchenObject().DestroySelf();
                        }
                    }
                }
            }
            else{
                //player has nothing!
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }
    }

}
