using map_app.Editing.Extensions;
using Mapsui;
using Mapsui.Nts.Extensions;
using Mapsui.UI.Avalonia;
using System;

namespace map_app.Editing;

public class EditManipulation
{
    private MPoint? _mouseDownPosition;
    private bool _inDoubleClick;

    public static int MinPixelsMovedForDrag { get; set; } = 4;

    public bool Manipulate(MouseState mouseState, MPoint screenPosition,
        EditManager editManager, MapControl mapControl)
    {
        switch (mouseState)
        {
            case MouseState.Up:
                if (_inDoubleClick) // Workaround to prevent that after a double click the 'up' event will immediately add a new geometry.
                    return (_inDoubleClick = false);

                if (editManager.EditMode == EditMode.Drag)
                    editManager.StopDragging();
                if (editManager.EditMode == EditMode.Rotate)
                    editManager.StopRotating();
                if (editManager.EditMode == EditMode.Scale)
                    editManager.StopScaling();

                if (IsClick(screenPosition, _mouseDownPosition))
                    return editManager.AddVertex(mapControl.Viewport.ScreenToWorld(screenPosition).ToCoordinate3D());

                return false;
            case MouseState.Down:
                {
                    _mouseDownPosition = screenPosition;
                    // Take into account VertexRadius in feature select, because the objective
                    // is to select the vertex.
                    var mapInfo = mapControl.GetMapInfo(screenPosition, editManager.VertexRadius);
                    if (mapInfo?.Feature == null)
                        return false;
                    return editManager.EditMode switch
                    {
                        EditMode.Drag => editManager.StartDraggingEntirely(mapInfo, editManager.VertexRadius),
                        EditMode.Rotate => editManager.StartRotating(mapInfo),
                        EditMode.Scale => editManager.StartScaling(mapInfo),
                        _ => false
                    };
                }
            case MouseState.Dragging:
                {
                    var args = mapControl.GetMapInfo(screenPosition);
                    return editManager.EditMode switch
                    {
                        EditMode.Drag => editManager.DraggingEntirely(args?.WorldPosition?.ToPoint()),
                        EditMode.Rotate => editManager.Rotating(args?.WorldPosition?.ToPoint()),
                        EditMode.Scale => editManager.Scaling(args?.WorldPosition?.ToPoint()),
                        _ => false
                    };
                }
            case MouseState.Moving:
                if (editManager.EditMode != EditMode.None && EditMode.DrawingMode.HasFlag(editManager.EditMode))
                    editManager.HoveringVertex(mapControl.GetMapInfo(screenPosition));
                return false;
            case MouseState.DoubleClick:
                _inDoubleClick = true;
                if (editManager.EditMode != EditMode.None && EditMode.DrawingMode.HasFlag(editManager.EditMode))
                {
                    editManager.EndEdit();
                    mapControl.RefreshGraphics();
                }
                return false;
            default:
                throw new Exception("Unknown mouse state");
        }
    }

    private static bool IsClick(MPoint? screenPosition, MPoint? mouseDownScreenPosition)
    {
        if (mouseDownScreenPosition == null || screenPosition == null)
            return false;
        return mouseDownScreenPosition.Distance(screenPosition) < MinPixelsMovedForDrag;
    }
}