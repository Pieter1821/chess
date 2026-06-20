using Godot;

public partial class CameraRig : Node3D
{
    // --- Tunables (editable in the Inspector) ---
    [Export] public float MinZoom = 4.0f;
    [Export] public float MaxZoom = 18.0f;
    [Export] public float DefaultZoom = 12.0f;
    [Export] public float ZoomStep = 1.0f;          // distance change per wheel notch
    [Export] public float MinPitch = 15.0f;         // degrees above the board (clamp = no clipping)
    [Export] public float MaxPitch = 80.0f;         // near top-down
    [Export] public float DefaultPitch = 50.0f;
    [Export] public float DefaultYaw = 0.0f;
    [Export] public float OrbitSensitivity = 0.4f;  // degrees per pixel dragged
    [Export] public float PanSensitivity = 0.01f;
    [Export] public float SmoothSpeed = 10.0f;      // higher = snappier easing

    private Camera3D _camera = null!;   // assigned in _Ready (runs before any use)
    private Vector3 _defaultPivot;

    // Where we want to be:
    private float _targetYaw, _targetPitch, _targetZoom;
    private Vector3 _targetPivot;
    // Where we currently are (eased toward the targets):
    private float _yaw, _pitch, _zoom;
    private Vector3 _pivot;

    private bool _orbiting, _panning;

    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("Camera3D");
        _defaultPivot = Position;            // the rig's start position = board center
        ResetToDefault(instant: true);
    }

    private void ResetToDefault(bool instant = false)
    {
        _targetYaw = DefaultYaw;
        _targetPitch = DefaultPitch;
        _targetZoom = DefaultZoom;
        _targetPivot = _defaultPivot;
        if (instant)
        {
            _yaw = _targetYaw; _pitch = _targetPitch; _zoom = _targetZoom; _pivot = _targetPivot;
            ApplyTransform();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton button)
        {
            switch (button.ButtonIndex)
            {
                case MouseButton.WheelUp when button.Pressed:
                    _targetZoom = Mathf.Clamp(_targetZoom - ZoomStep, MinZoom, MaxZoom);
                    break;
                case MouseButton.WheelDown when button.Pressed:
                    _targetZoom = Mathf.Clamp(_targetZoom + ZoomStep, MinZoom, MaxZoom);
                    break;
                case MouseButton.Right:
                    _orbiting = button.Pressed;   // held = true, released = false
                    break;
                case MouseButton.Middle:
                    _panning = button.Pressed;
                    break;
            }
        }
        else if (@event is InputEventMouseMotion motion)
        {
            if (_orbiting)
            {
                _targetYaw -= motion.Relative.X * OrbitSensitivity;
                _targetPitch = Mathf.Clamp(_targetPitch + motion.Relative.Y * OrbitSensitivity, MinPitch, MaxPitch);
            }
            else if (_panning)
            {
                Pan(motion.Relative);
            }
        }
        else if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.R)
        {
            ResetToDefault();
        }
    }

    private void Pan(Vector2 screenDelta)
    {
        // Move the pivot along the ground, relative to where we're looking.
        Basis basis = GlobalTransform.Basis;
        Vector3 right = basis.X; right.Y = 0; right = right.Normalized();
        Vector3 forward = basis.Z; forward.Y = 0; forward = forward.Normalized();
        float scale = PanSensitivity * _zoom;   // pan faster when zoomed out
        _targetPivot += (-screenDelta.X * right + screenDelta.Y * forward) * scale;
    }

    public override void _Process(double delta)
    {
        float t = Mathf.Clamp((float)delta * SmoothSpeed, 0f, 1f);
        _yaw = Mathf.Lerp(_yaw, _targetYaw, t);
        _pitch = Mathf.Lerp(_pitch, _targetPitch, t);
        _zoom = Mathf.Lerp(_zoom, _targetZoom, t);
        _pivot = _pivot.Lerp(_targetPivot, t);
        ApplyTransform();
    }

    private void ApplyTransform()
    {
        Position = _pivot;                                  // pan
        RotationDegrees = new Vector3(-_pitch, _yaw, 0f);   // orbit (negative pitch lifts the camera up)
        _camera.Position = new Vector3(0f, 0f, _zoom);      // zoom (distance back from pivot)
    }
}
