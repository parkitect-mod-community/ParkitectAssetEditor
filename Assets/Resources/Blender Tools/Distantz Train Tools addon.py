import bpy
from bpy.app.handlers import persistent

bl_info = {
    "name": "Distantz Train Tools",
    "category": "Rigging",
    "description": "A tool that enables looping bones on curves with offsets",
    "blender" : (2, 80, 0)
}

def update(self, context):
    bpy.context.area.tag_redraw()
    bpy.app.driver_namespace['getLocationOnTrack'] = getLocationOnTrack

    try:

        activePoseBone = bpy.context.active_pose_bone 
        
        bpy.data.objects[bpy.context.active_object.name].data.bones[bpy.data.objects[bpy.context.active_object.name].data.bones[0].name].select = True
        bpy.data.objects[bpy.context.active_object.name].data.bones[bpy.data.objects[bpy.context.active_object.name].data.bones[0].name].select = False
        bpy.data.objects[bpy.context.active_object.name].data.bones[bpy.data.objects[bpy.context.active_object.name].data.activePoseBone].select = True

    except:
        pass

@persistent 
def doMenuUpdate(scene, context):
    try:
        bpy.context.area.tag_redraw()
    except:
        pass
    bpy.app.driver_namespace['getLocationOnTrack'] = getLocationOnTrack

def getLocationOnTrack(offset):
    
    currentPos = (bpy.context.scene.positionOnTrack + bpy.context.scene.shift) - (bpy.context.scene.offset * offset)

    if currentPos < round(currentPos):

        currentPos -= (round(currentPos) - 1)

    elif currentPos >= round(currentPos):

        currentPos -= round(currentPos)
    
    return currentPos

class ApplyDriverOperator(bpy.types.Operator):
    """Tooltip"""
    bl_idname = "object.apply_driver"
    bl_label = "Apply Drivers"

    @classmethod
    def poll(cls, context):
        return context.active_object is not None

    def execute(self, context):
        applyDrivers(context)
        return {'FINISHED'}

class RotateChassisOperator(bpy.types.Operator):
    """Tooltip"""
    bl_idname = "object.rotate_chassis"
    bl_label = "Rotate Chassis"

    @classmethod
    def poll(cls, context):
        return context.active_object is not None

    def execute(self, context):
        applyChassis(context)
        return {'FINISHED'}
    
class TitleHeader(bpy.types.Panel):
    bl_idname = "DTT_PT_PanelTitle"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "DTT"
    bl_context = "posemode"
    bl_label = "Distantz Train Tools"



    def draw_header(self, context):
        layout = self.layout

    def draw(self, context):
        
        scene = context.scene
        
        ## Start layout
        layout = self.layout
        
        ## Track Options row
        descRow = layout.row()
        desc = descRow.label(text="Animate using Lead Car Position")

        
class ObjectSelectPanel(bpy.types.Panel):
    bl_idname = "DTT_PT_Panel"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "DTT"
    bl_context = "posemode"
    bl_label = "Track Options"


    def draw(self, context):
        
        scene = context.scene
        
        ## Start layout
        layout = self.layout
        
        ## Track Options row
        posRow = layout.row()
        posRow.prop(context.scene, 'positionOnTrack', slider=True)
        offsetRow = layout.row()
        offsetRow.prop(context.scene, 'offset', slider=True)
        shiftRow = layout.row()
        shiftRow.prop(context.scene, 'shift', slider=True)

    def check(self, context):
        if self.use_count:
            return True
        return False
        
        
class ObjectSelectPanel2(bpy.types.Panel):
    bl_idname = "DTT_PT_Panel2"
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "DTT"
    bl_context = "posemode"
    bl_label = "Drivers"
    
        
    def draw(self, context):
        
        scene = context.scene
        
        ## Start layout
        layout = self.layout
        
        buttonRow = layout.row()
        buttonRow.operator("object.apply_driver", text = "Apply Drivers")
        buttonRow2 = layout.row()
        buttonRow2.prop_search(scene, "selectedCurve", scene, "objects", text = "Curve")
        buttonRow.operator("object.rotate_chassis", text = "Auto Chassis")
        
        

def applyChassis(context):
    
    name = bpy.context.active_pose_bone.name.split(".")[0]
    
    for object in bpy.context.selected_pose_bones:
        
        try:
            
            number = int(object.name.split(".")[1].lstrip("0")) - 1
            
        except:
            
            number = -1

        print(number, name)

        if number != -1 and number != 0:

            targetObject = bpy.data.objects[object.id_data.name]
            
            crc = object.constraints.new(type='DAMPED_TRACK')
            crc.target = targetObject
            
            crc.subtarget = str("{}.{}".format(object.name.split(".")[0], str(number).zfill(3)))

        elif number == 0:

            targetObject = bpy.data.objects[object.id_data.name]
            
            crc = object.constraints.new(type='DAMPED_TRACK')
            crc.target = targetObject
            crc.subtarget = str("{}".format(object.name.split(".")[0]))
            
        try:
            print("done", crc.subtarget)

        except:
            print("Not done")


def applyDrivers(context):
    
    def add_drivers(object, offset):
        
        d = bpy.data.objects[object.id_data.name].pose.bones[object.name].constraints["DTT Follow Path"].driver_add('offset_factor')
        
        driver = d.driver
        
        driver.expression = "getLocationOnTrack({})".format(offset)
        

    for object in bpy.context.selected_pose_bones:
        
        try:
            
            offset = object.name.split(".")[1].lstrip("0")
            
        except:
            
            offset = 0
            
        copyLocConstraints = [ c for c in object.constraints if c.name == "DTT Follow Path"]

        for c in copyLocConstraints:
            object.constraints.remove( c ) # Remove constraint
        
        crc = object.constraints.new(type='FOLLOW_PATH')
        
        crc.use_fixed_location = True
        crc.name = "DTT Follow Path"
        crc.offset_factor = 0.69
        crc.use_curve_follow = True
        crc.target = bpy.data.objects[context.scene.selectedCurve]
        crc.forward_axis = "FORWARD_X"
        
        add_drivers(object, offset)


def register():
    prehandle = bpy.app.handlers.depsgraph_update_pre
    prehandle.append(addDriverNamespace)

    prehandle2 = bpy.app.handlers.frame_change_post
    prehandle2.append(doMenuUpdate)

    bpy.types.Scene.positionOnTrack = bpy.props.FloatProperty(update = update, name="Lead Car Position", description="Sets the position of the train on the track", default=0, soft_min=-1, soft_max=1)
    bpy.types.Scene.offset = bpy.props.FloatProperty(update = update, name="Car Offsets", description="Sets the offsets between cars", default=0, soft_min=0, max=1)
    bpy.types.Scene.shift = bpy.props.FloatProperty(update = update, name="Shift", description="Sets the shift of the lead car position", default=0, soft_min=-1, soft_max=1)
    bpy.types.Scene.selectedCurve = bpy.props.StringProperty()
    bpy.utils.register_class(ApplyDriverOperator)
    bpy.utils.register_class(RotateChassisOperator)
    bpy.utils.register_class(TitleHeader)
    bpy.utils.register_class(ObjectSelectPanel)
    bpy.utils.register_class(ObjectSelectPanel2)

@persistent
def addDriverNamespace(scene):
    bpy.app.driver_namespace['getLocationOnTrack'] = getLocationOnTrack
    
def unregister():
    pass

if __name__ == "__main__":
    register()
