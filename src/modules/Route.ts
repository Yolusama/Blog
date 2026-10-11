export class Route{
      static switch(url:string){
          window.location.replace(url);
      }
  
      static dive(url:string){
          window.location.assign(url);
      }
  
      static open(url:string){
          window.open(url);
      }
  
      static back(){
          window.history.back();
      }
  
      static forward(){
          window.history.forward();
      }
  
      static go(delta:number){
          window.history.go(delta);
      } 
}