using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress{

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public class OnStateChangedEventArgs : EventArgs{
        public State state;
    }

    public enum State {
        Idle,
        Frying,
        Fried,
        Burned,
    }

    [SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private float fryingTimer;
    private float burningTimer;
    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;
    private State state;

    private void Start(){
        state = State.Idle;
    }

    private void Update(){
        if(HasKitchenObject()){
            switch (state){
                case State.Idle:
                    break;
                
                case State.Frying:
                    fryingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalised = (float)fryingTimer / fryingRecipeSO.fryingTimerMax
                    });

                    if(fryingTimer > fryingRecipeSO.fryingTimerMax){
                        //fried!
                        KitchenObjectSO cookedKitchenObjectSO = fryingRecipeSO.output;
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(cookedKitchenObjectSO, this);

                        state = State.Fried;
                        burningTimer = 0f;
                        burningRecipeSO = GetBurningRecipeSOWithInput(cookedKitchenObjectSO);

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs{
                            state = state
                        });
                    }
                    
                    break;

                case State.Fried:
                    burningTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                        progressNormalised = (float)burningTimer / burningRecipeSO.burningTimerMax
                    });

                    if(burningTimer > burningRecipeSO.burningTimerMax){
                        //burned!
                        KitchenObjectSO burnedKitchenObjectSO = burningRecipeSO.output;
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(burnedKitchenObjectSO, this);
                        
                        state = State.Burned;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs{
                            state = state
                        });

                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                            progressNormalised = 0f
                        });
                    }

                    break;

                case State.Burned:
                    break;            
            }
        }
    }

    public override void Interact(Player player){
        //ClearCounter logic to just drop what player is carrying:
        if(!HasKitchenObject()){
            //there is no kitchen object here!
            if(player.HasKitchenObject()){
                //player is carrying smthing!
                if(HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())){
                    //player is carrying smthing valid from the recipes!
                    player.GetKitchenObject().SetKitchenObjectParent(this);

                    if (state != State.Burned){
                        state = State.Frying;
                        fryingTimer = 0f;
                        fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs{
                            state = state
                        });
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                            progressNormalised = (float)fryingTimer / fryingRecipeSO.fryingTimerMax
                        });
                    }
                }
                else{
                    //player carried object not in any valid recipe
                }
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
                    if(plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())){
                        GetKitchenObject().DestroySelf();
                        
                        state = State.Idle;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs{
                            state = state
                        });
                        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                            progressNormalised = 0f
                        });
                    }
                }
                        
            }
            else{
                //player has nothing!
                GetKitchenObject().SetKitchenObjectParent(player);

                state = State.Idle;

                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs{
                    state = state
                });
                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                    progressNormalised = 0f
                });
            }
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO){
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        if (fryingRecipeSO != null){
            return true;
        }
        else{
            return false;
        }
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO){
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
        if (fryingRecipeSO != null){
            return fryingRecipeSO.output;
        }
        else{
            return null;
        }
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO){
        foreach(FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray){
            if(fryingRecipeSO.input == inputKitchenObjectSO){
                return fryingRecipeSO;
            }
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO){
        foreach(BurningRecipeSO burningRecipeSO in burningRecipeSOArray){
            if(burningRecipeSO.input == inputKitchenObjectSO){
                return burningRecipeSO;
            }
        }
        return null;
    }

}
