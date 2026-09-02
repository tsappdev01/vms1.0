#ifndef _ANSI_NIST_ITL_1_H_
#define _ANSI_NIST_ITL_1_H_

#include <stdint.h>
#include <stdbool.h>

#ifdef __cplusplus
#define ENUM_DECLARATION enum struct
#else
#define ENUM_DECLARATION enum
#endif

#ifdef __cplusplus
namespace Tech5_core {
#endif // __cplusplus
// ANSI/NIST-ITL 1-2011 Update:2015
// Data Format for the Interchange of Fingerprint, Facial & Other Biometric Information


/** @brief NIST_IMP_TYPE
    @class NIST_IMP_TYPE
    Impression type enumeration

CT  - contact
CS  - contactless
FP  - fingerprint    - an image or impression of the friction ridges of all or any part of a finger or thumb).
FR  - friction ridge - an image of an impression from the palmar surfaces of the hands or fingers, or from the plantar (sole) surfaces of the feet or toes.
IMP - impression     - a friction ridge image containing friction ridge detail produced on a surface by pressure.
LT  - latent         - an impression or image of friction ridge skin left on a surface.

      A latent impression is the digital image of the latent impression that was acquired directly from a latent impression, using a flatbed scanner or digital camera.
      Latent tracing is when the digital image is of a drawn tracing of the impression, not the impression itself. The tracing may have been hand or computer-drawn.
      A latent photo means that the digital image was acquired from a paper photograph that had been taken off a latent impression; the paper photograph was then digitized using a flatbed scanner or digital camera.
      A latent lift is a digital image that was acquired from a lift of the latent impression, using a flatbed scanner or digital camera.

LS  - livescan
MS  - multispectral
OP  - optical
PL  - plain          - a fingerprint image resulting from the touching of one or more fingers to a livescan platen or paper fingerprint card without any rolling motion.
RD  - rolled         - a fingerprint image collected by rolling the finger across a livescan platen or paper fingerprint card from nail to nail.
U   - unknown
V   - vertical

palm print - a friction ridge image from the palm (side and underside) of the hand. A full palm print includes the area from the wrist to the tips of the fingers.
plantar    - the friction ridge skin on the feet (soles and toes).
*/

ENUM_DECLARATION NIST_IMP_TYPE
{
    /// livescan (type unknown or unspecified) plain fingerprint
    IMP_TYPE_FP_LS_PL           = 0,
    /// livescan (type unknown or unspecified) rolled fingerprint
    IMP_TYPE_FP_LS_RD           = 1,
    /// non-livescan (e.g., inked) plain fingerprint
    IMP_TYPE_FP_NON_LS_PL       = 2,
    /// non-livescan (e.g., inked) rolled fingerprint
    IMP_TYPE_FP_NON_LS_RD       = 3,
    /// latent fingerprint impression
    IMP_TYPE_FP_LT_IMP          = 4,
    /// latent fingerprint tracing
    IMP_TYPE_FP_LT_TRACING      = 5,
    /// latent fingerprint photo
    IMP_TYPE_FP_LT_PHOTO        = 6,
    /// latent fingerprint lift
    IMP_TYPE_FP_LT_LIFT         = 7,
    /// livescan fingerprint vertical swipe
    IMP_TYPE_FP_LS_V_SWIPE      = 8,

    // 9                               // n/a
    /// livescan (type unknown or unspecified) palm
    IMP_TYPE_PALM_LS            = 10,
    /// non-livescan (e.g., inked) palm
    IMP_TYPE_PALM_NON_LS        = 11,
    /// latent palm impression
    IMP_TYPE_PALM_LT_IMP        = 12,
    /// latent palm tracing
    IMP_TYPE_PALM_LT_TRACING    = 13,
    /// latent palm photo
    IMP_TYPE_PALM_LT_PHOTO      = 14,
    /// latent palm lift
    IMP_TYPE_PALM_LT_LIFT       = 15,

    // 16-19                           // n/a
    /// livescan optical contact plain fingerprint
    IMP_TYPE_FP_LS_OP_CT_PL     = 20,
    /// livescan optical contact rolled fingerprint
    IMP_TYPE_FP_LS_OP_CT_RD     = 21,
    /// livescan non-optical contact plain fingerprint
    IMP_TYPE_FP_LS_NON_OP_CT_PL = 22,
    /// livescan non-optical contact rolled fingerprint
    IMP_TYPE_FP_LS_NON_OP_CT_RD = 23,
    /// livescan optical contactless plain fingerprint
    IMP_TYPE_FP_LS_OP_CS_PL     = 24,
    /// livescan optical contactless rolled fingerprint
    IMP_TYPE_FP_LS_OP_CS_RD     = 25,
    /// livescan non-optical contactless plain fingerprint
    IMP_TYPE_FP_LS_NON_OP_CS_PL = 26,
    /// livescan non-optical contactless rolled fingerprint
    IMP_TYPE_FP_LS_NON_OP_CS_RD = 27,

    /// other
    IMP_TYPE_OTHER              = 28,
    /// unknown
    IMP_TYPE_UNKNOWN            = 29,

    /// livescan (type unknown or unspecified) plantar
    IMP_TYPE_PLANTAR_LS         = 30,
    /// non-livescan (e.g., inked) plantar
    IMP_TYPE_PLANTAR_NON_LS     = 31,
    /// latent plantar impression
    IMP_TYPE_PLANTAR_LT_IMP     = 32,
    /// latent plantar tracing
    IMP_TYPE_PLANTAR_LT_TRACING = 33,
    /// latent plantar photo
    IMP_TYPE_PLANTAR_LT_PHOTO   = 34,
    /// latent plantar lift
    IMP_TYPE_PLANTAR_LT_LIFT    = 35,

    /// latent unknown friction ridge impression
    IMP_TYPE_U_FR_LT_IMP        = 36,
    /// latent unknown friction ridge tracing
    IMP_TYPE_U_FR_LT_TRACING    = 37,
    /// latent unknown friction ridge photo
    IMP_TYPE_U_FR_LT_PHOTO      = 38,
    /// latent unknown friction ridge lift
    IMP_TYPE_U_FR_LT_LIFT       = 39,

    /// livescan optical multispectral plain fingerprint
    IMP_TYPE_FP_LS_OP_MS_PL     = 40,
    /// livescan optical multispectral rolled fingerprint
    IMP_TYPE_FP_LS_OP_MS_RD     = 41
};


/** @brief NIST_POS_CODE
    @class NIST_POS_CODE
   Friction ridge position enumeration

EJI - entire joint image - an exemplar image containing all four full-finger views for a single finger.
F   - finger
FR  - friction ridge     - an image of an impression from the palmar surfaces of the hands or fingers, or from the plantar (sole) surfaces of the feet or toes.
FT  - fingertips
INC - include
L   - left
PL  - plain
POS - position
R   - right
4F  - four fingers
U   - unknown
WR  - writer's

Codes 11, 12, 13, 14, 15 and 40-54 do not apply to latent prints.
Code 46 right index / left index means that the right index placed on the right portion of the imaging area and the left index on the left portion of that same imaging area.
*/
ENUM_DECLARATION NIST_POS_CODE
{
    /// Finger position codes
    /// unknown finger
    POS_CODE_U_FINGER             = 0,
    /// right thumb
    POS_CODE_R_THUMB              = 1,
    /// right index finger
    POS_CODE_R_INDEX_F            = 2,
    /// right middle finger
    POS_CODE_R_MIDDLE_F           = 3,
    /// right ring finger
    POS_CODE_R_RING_F             = 4,
    /// right little finger
    POS_CODE_R_LITTLE_F           = 5,
    /// left thumb
    POS_CODE_L_THUMB              = 6,
    /// left index finger
    POS_CODE_L_INDEX_F            = 7,
    /// left middle finger
    POS_CODE_L_MIDDLE_F           = 8,
    /// left ring finger
    POS_CODE_L_RING_F             = 9,
    /// left little finger
    POS_CODE_L_LITTLE_F           = 10,
    /// plain right thumb
    POS_CODE_PL_R_THUMB           = 11,
    /// plain left thumb
    POS_CODE_PL_L_THUMB           = 12,
    /// plain right four fingers (may include extra digits)
    POS_CODE_PL_R_4F              = 13,
    /// plain left four fingers (may include extra digits)
    POS_CODE_PL_L_4F              = 14,
    /// left & right thumbs
    POS_CODE_L_AND_R_THUMBS       = 15,
    /// right extra digit
    POS_CODE_R_EXTRA_DIGIT        = 16,
    /// left extra digit
    POS_CODE_L_EXTRA_DIGIT        = 17,
    /// unknown friction ridge
    POS_CODE_U_FR                 = 18,
    /// entire joint image or tip
    POS_CODE_EJI_OR_TIP           = 19,

    /// Palm position codes
    /// unknown palm
    POS_CODE_U_PALM               = 20,
    /// right full palm
    POS_CODE_R_FULL_PALM          = 21,
    /// right writer's palm
    POS_CODE_R_WR_PALM            = 22,
    /// left full palm
    POS_CODE_L_FULL_PALM          = 23,
    /// left writer's palm
    POS_CODE_L_WR_PALM            = 24,
    /// right lower palm
    POS_CODE_R_LOWER_PALM         = 25,
    /// right upper palm
    POS_CODE_R_UPPER_PALM         = 26,
    /// left lower palm
    POS_CODE_L_LOWER_PALM         = 27,
    /// left upper palm
    POS_CODE_L_UPPER_PALM         = 28,
    /// right other
    POS_CODE_R_OTHER              = 29,
    /// left other
    POS_CODE_L_OTHER              = 30,
    /// right interdigital
    POS_CODE_R_INTERDIGITAL       = 31,
    /// right thenar
    POS_CODE_R_THENAR             = 32,
     /// right hypothenar
    POS_CODE_R_HYPOTHENAR         = 33,
    /// left interdigital
    POS_CODE_L_INTERDIGITAL       = 34,
    /// left thenar
    POS_CODE_L_THENAR             = 35,
    /// left hypothenar
    POS_CODE_L_HYPOTHENAR         = 36,
    /// right grasp
    POS_CODE_R_GRASP              = 37,
    /// left grasp
    POS_CODE_L_GRASP              = 38,

    // 39

    /// Multiple finger position codes. 2-Finger combinations
    /// right index/middle
    POS_CODE_R_INDEX_MIDDLE       = 40,
     /// right middle/ring
    POS_CODE_R_MIDDLE_RING        = 41,
    /// right ring/little
    POS_CODE_R_RING_LITTLE        = 42,
    /// left index/middle
    POS_CODE_L_INDEX_MIDDLE       = 43,
    /// left middle/ring
    POS_CODE_L_MIDDLE_RING        = 44,
    /// left ring/little
    POS_CODE_L_RING_LITTLE        = 45,
    /// right index / left index
    POS_CODE_R_INDEX_L_INDEX      = 46,

    /// Multiple finger position codes. 3-Finger combinations
    /// right index/middle/ring
    POS_CODE_R_INDEX_MIDDLE_RING  = 47,
    /// right middle/ring/little
    POS_CODE_R_MIDDLE_RING_LITTLE = 48,
    /// left index/middle/ring
    POS_CODE_L_INDEX_MIDDLE_RING  = 49,
    /// left middle/ring/little
    POS_CODE_L_MIDDLE_RING_LITTLE = 50,

    /// Multiple finger position codes. 4-Finger combinations. Note codes 13 and 14
    /// fingertips (4 fingers simultaneously  no thumb  right hand - plain)
    POS_CODE_FT_PL_R_4F_NO_THUMB  = 51,
    /// fingertips (4 fingers simultaneously  no thumb left hand - plain)
    POS_CODE_FT_PL_L_4F_NO_THUMB  = 52,

    /// Multiple finger position codes. 5-Finger combinations
    /// fingertips (4 fingers and thumb simultaneously right hand - plain)
    POS_CODE_FT_PL_R_4F_AND_THUMB = 53,
    /// fingertips (4 fingers and thumb simultaneously left hand - plain)
    POS_CODE_FT_PL_L_4F_AND_THUMB = 54,

    // 55-59

    /// Plantar position codes
    /// unknown sole
    POS_CODE_U_SOLE               = 60,
    /// sole right foot
    POS_CODE_SOLE_R_FOOT          = 61,
    /// sole left foot
    POS_CODE_SOLE_L_FOOT          = 62,
    /// unknown toe
    POS_CODE_U_TOE                = 63,
    /// right big toe
    POS_CODE_R_BIG_TOE            = 64,
    /// right second toe
    POS_CODE_R_SECOND_TOE         = 65,
    /// right middle toe
    POS_CODE_R_MIDDLE_TOE         = 66,
    /// right fourth toe
    POS_CODE_R_FOURTH_TOE         = 67,
    /// right little toe
    POS_CODE_R_LITTLE_TOE         = 68,
    /// left big toe
    POS_CODE_L_BIG_TOE            = 69,
    /// left second toe
    POS_CODE_L_SECOND_TOE         = 70,
    /// left middle toe
    POS_CODE_L_MIDDLE_TOE         = 71,
    /// left fourth toe
    POS_CODE_L_FOURTH_TOE         = 72,
    /// left little toe
    POS_CODE_L_LITTLE_TOE         = 73,
    /// front / ball of right foot
    POS_CODE_FRONT_BALL_OF_R_FOOT = 74,
    /// back / heel of right foot
    POS_CODE_BACK_HEEL_OF_R_FOOT  = 75,
    /// front / ball of left foot
    POS_CODE_FRONT_BALL_OF_L_FOOT = 76,
    /// back / heel of left foot
    POS_CODE_BACK_HEEL_OF_L_FOOT  = 77,
    /// right middle of foot
    POS_CODE_R_MIDDLE_OF_FOOT     = 78,
    /// left middle of foot
    POS_CODE_L_MIDDLE_OF_FOOT     = 79,

    // 80

    /// Palm position codes
    /// right carpal delta area
    POS_CODE_R_CARPAL_DELTA_AREA  = 81,
    /// left carpal delta area
    POS_CODE_L_CARPAL_DELTA_AREA  = 82,
    /// right full palm, including writer's palm
    POS_CODE_R_FULL_PALM_INC_WR   = 83,
    /// left full palm, including writer's palm
    POS_CODE_L_FULL_PALM_INC_WR   = 84,
    /// right wrist bracelet
    POS_CODE_R_WRIST_BRACELET     = 85,
    /// left wrist bracelet
    POS_CODE_L_WRIST_BRACELET     = 86
};

#ifdef __cplusplus
} // namespace Tech5_core {
#endif //__cplusplus

#endif//_ANSI_NIST_ITL_1_H_
