class StateStroge
{
    private state:Record<string,any> = {};
    constructor()
    {
        const value = localStorage.getItem("state");
        if(value==null) this.state = {};
        else this.state = JSON.parse(value);
    }

    set(key:string,value:any)
    {
        this.state[key] = value;
        localStorage.setItem("state",JSON.stringify(this.state));
    }
    
    get<T>(key:string):T
    {
       return this.state[key] as T;
    }

    remove(key:string)
    {
        const state:Record<string,any> = {};
        for(const pro in this.state)
        {
            if(pro!=key)
                state[pro] = this.state[pro];
        }
        this.state = state;
        localStorage.setItem("state", JSON.stringify(this.state));
    }
    
    has(key:string)
    {
        return this.get(key)!=undefined;
    }
    
    setState(data:Record<string,any>)
    {
        this.state = data;
        localStorage.setItem("state",JSON.stringify(this.state));
    }

    clear()
    {
        this.state = {};
        localStorage.clear();
    }

}

const stateStroge = new StateStroge();

export default stateStroge;