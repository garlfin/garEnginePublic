namespace gESilk.engine.components;

public class CubemapCapture : Component
{
    private float _yaw, _pitch;
    
    public CubemapCapture()
    {
    }

    public void GetAngle(int index)
    {
        switch (index)
        {
            case 0:
                _pitch = 0;
                _yaw = 90;
                break;
            case 1:
                _pitch = 0;
                _yaw = -90;
                break;
            case 2:
                _pitch = -90;
                _yaw = 180;
                break;
            case 3:
                _pitch = 90;
                _yaw = 180;
                break;
            case 4:
                _pitch = 0;
                _yaw = 180;
                break;
            case 5:
                _pitch = 0;
                _yaw = 0;
                break;
        }
    }

    public override void Update(float gameTime)
    {
       
    }
    

    public override void UpdateMouse(float gameTime)
    {
        
    }
}