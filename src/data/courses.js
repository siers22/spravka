import * as systems from './lessons.js'
import * as programmer from './programmer-lessons.js'
import {codes as systemsCodes} from './code.js'
import {codes as programmerCodes} from './programmer-code.js'
import {courseMeta} from './course-meta.js'
export const courses={
'information-systems':{...courseMeta['information-systems'],...systems,codes:systemsCodes},
programmer:{...courseMeta.programmer,...programmer,codes:programmerCodes}}
