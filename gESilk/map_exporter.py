import bpy
import struct
from mathutils import Vector
import shutil
import os

exportFolder = "C:/Users/scion/RiderProjects/garEngine/gESilk/resources/maps/"
myFile = open(exportFolder + "test.map", "wb")

def radToDeg(input):
    return input * (180 / 3.14159265359)

def packVector3(data: Vector, convert = False):
    if convert:
        return struct.pack('fff', data.x, data.z, -data.y)
    else:
        return struct.pack('fff', data.x, data.y, data.z)

def packVector3Rad(data: Vector):
    return struct.pack('fff', radToDeg(data.x), radToDeg(data.y), radToDeg(data.z))

def packString(string_data):
    
 
    data = struct.pack('i', len(string_data))
    
    for character in string_data:
        data += struct.pack('c', character.encode('ascii'))
        
    return data

def packTransform(object):
    data = packVector3(item.location, True)
    if object.type == "CAMERA":
        data += packVector3Rad(Vector((0.0,0.0,0.0)))
    else:
        data += packVector3Rad(item.rotation_euler)
        
    if object.type == "LIGHT_PROBE":
        data += packVector3(Vector((item.scale.x, item.scale.z, item.scale.y)) * item.data.influence_distance)
    else:
        data += packVector3(item.scale)
    return data

def copyPathReturn(originalPath):
    finalpath = exportFolder + "../texture/" + os.path.basename(originalPath)
    try:
        shutil.copyfile(bpy.path.abspath(originalPath), finalpath)
    except:
        print("COULD NOT COPY")
    finally:
        return os.path.basename(originalPath)

final_data = b''
tot_mat = 0

for material in bpy.data.materials:
    if material.name == "Dots Stroke":
        continue
    tot_mat += 1
    
    final_data += packString("MATERIAL")
    final_data += packString(material.name)
    
    principled = material.node_tree.nodes["Principled BSDF"]

    final_data += packString("albedo")
    final_data += packString("../../../resources/texture/" + copyPathReturn(principled.inputs['Base Color'].links[0].from_node.inputs["Color1"].links[0].from_node.image.filepath))

    final_data += packString("specularTex")
    final_data += packString("../../../resources/texture/" + copyPathReturn(principled.inputs['Roughness'].links[0].from_node.inputs['Image'].links[0].from_node.image.filepath))

    final_data += packString("normalMap")
    final_data += packString("../../../resources/texture/" + copyPathReturn(principled.inputs['Normal'].links[0].from_node.inputs['Color'].links[0].from_node.image.filepath))
    final_data += struct.pack('f', principled.inputs['Normal'].links[0].from_node.inputs['Strength'].default_value)
    
myFile.write(struct.pack('i', tot_mat) + final_data)

final_data = b''
       
tot = 0
for item in bpy.context.scene.objects:
    if item.type == "LIGHT_PROBE" or item.type == "CAMERA" or item.type == "LIGHT":
        tot += 1
        final_data += packString(item.type)
        final_data += packString(item.name)
        final_data += packTransform(item)
        if item.type == "LIGHT":
            final_data += packString(item.data.type)
            if item.data.type == "POINT":
                final_data += struct.pack("fffff", item.data.energy, item.data.shadow_soft_size, item.data.color.r, item.data.color.g, item.data.color.b )
    if item.type == "MESH":
        bpy.ops.object.select_all(action='DESELECT')
        item.select_set(state=True)
        bpy.ops.transform.rotate(value=-1.5708, orient_axis='X', orient_type='GLOBAL', orient_matrix=((1, 0, 0), (0, 1, 0), (0, 0, 1)), orient_matrix_type='GLOBAL', constraint_axis=(True, False, False), mirror=False, use_proportional_edit=False, proportional_edit_falloff='SMOOTH', proportional_size=1, use_proportional_connected=False, use_proportional_projected=False)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        tot += 1
        final_data += packString(item.type)
        final_data += packString(item.name)
        final_data += packTransform(item)
    
        exportName = exportFolder + "../models/" + item.name + '.fbx'
        bpy.ops.export_scene.fbx(filepath=exportName, use_selection=True, axis_forward='Y', axis_up='Z')
        final_data += packString("../../../resources/models/" + item.name + ".fbx")
         
        final_data += struct.pack('i', len(item.material_slots))
        for material in item.material_slots:
            final_data += packString(material.name)
        bpy.ops.transform.rotate(value=1.5708, orient_axis='X', orient_type='GLOBAL', orient_matrix=((1, 0, 0), (0, 1, 0), (0, 0, 1)), orient_matrix_type='GLOBAL', constraint_axis=(True, False, False), mirror=False, use_proportional_edit=False, proportional_edit_falloff='SMOOTH', proportional_size=1, use_proportional_connected=False, use_proportional_projected=False)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
myFile.write(struct.pack('i', tot) + final_data)

myFile.close()