import bpy
import struct
from mathutils import Vector
import shutil
import os

exportFolder = "C:/Users/scion/Documents/GitHub/garEnginePublic/gESilk/resources/maps/"
myFile = open(exportFolder + "test.map", "wb")

def radToDeg(input):
    return input * (180 / 3.14159265359)

def packVector3(data: Vector, scale = 1):
    return struct.pack('fff', data.x, data.y, data.z)

def packVector3Rad(data: Vector):
    return struct.pack('fff', radToDeg(data.x), radToDeg(data.y), radToDeg(data.z))

def packString(string_data):
    
 
    data = struct.pack('i', len(string_data))
    
    for character in string_data:
        data += struct.pack('c', character.encode('ascii'))
        
    return data

def packTransform(object):
    data = packVector3(item.location)
    if object.type == "CAMERA":
        data += packVector3Rad(Vector((0.0,0.0,0.0)))
    else:
        data += packVector3Rad(item.rotation_euler)
    data += packVector3(item.scale, item.data.influence_distance if object.type == "LIGHT_PROBE" else 1)
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

    final_data += packString("ALBEDO")
    final_data += packString("../../../resources/texture/" + copyPathReturn(principled.inputs['Base Color'].links[0].from_node.image.filepath))

    final_data += packString("SPECULAR")
    final_data += packString("../../../resources/texture/" + copyPathReturn(principled.inputs['Roughness'].links[0].from_node.image.inputs['Color'].links[0].from_node.image.filepath))

    final_data += packString("NORMAL")
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
        
    if item.type == "MESH":
        tot += 1
        final_data += packString(item.type)
        final_data += packString(item.name)
        final_data += packTransform(item)
    
        bpy.ops.object.select_all(action='DESELECT')
        item.select_set(state=True)
        exportName = exportFolder + "../models/" + item.name + '.fbx'
        bpy.ops.export_scene.fbx(filepath=exportName, use_selection=True, axis_forward='Z', axis_up='Y')
        final_data += packString("../../../resources/models/" + item.name + ".fbx")
         
        final_data += struct.pack('i', len(item.material_slots))
        for material in item.material_slots:
            final_data += packString(material.name)
             
myFile.write(struct.pack('i', tot) + final_data)

myFile.close()