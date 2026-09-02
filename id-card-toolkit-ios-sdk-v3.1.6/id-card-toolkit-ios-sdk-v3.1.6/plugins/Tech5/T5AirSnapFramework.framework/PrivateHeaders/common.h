/*****************************************************************************
   common.h - the common header file

*******************************************************************************/
 
#ifndef COMMON_H__
#define COMMON_H__

#include <stdint.h>
#include <stdbool.h>
#include <string.h>
#include <assert.h>

#include "win2lin.h"
#include "ansi_nist_itl_1.h"

#if defined __cplusplus
namespace Tech5_core {
#endif

// packing defines
#define _CORE_TIGHT      1
#define _CORE_PACKING    8


#ifndef __cplusplus
//   typedef char      bool;
   #define true          1
   #define false         0 
#endif

#define _DBG_INFO
#define _DBG_INFO_1
#define _DBG_INFO_2

/// maximum number of minutiae in fingerprint template
#define MAX_MINUTIAE    255
#ifndef MAX_FINGERS
#define MAX_FINGERS     10
#endif // MAX_FINGERS

#ifdef __cplusplus
   extern "C"{
#endif
/******************************************************************
          THE CONSTANRS
******************************************************************/
/** @brief MATCH_SPEED 
    @class MATCH_SPEED 
    Matching speed constants enumeration
*/
ENUM_DECLARATION MATCH_SPEED
{
   LOWEST_MATCH_SPEED    = 0,
   LOW_MATCH_SPEED       = 1,
   REDUCED_MATCH_SPEED   = 2,
   NORMAL_MATCH_SPEED    = 3,
   INCREASED_MATCH_SPEED = 4,
   HIGH_MATCH_SPEED      = 5,
   HIGHEST_MATCH_SPEED   = 6,
};

/** @brief NIST_QUALITY
    @class NIST_QUALITY
    NIST quality constants enumeration
*/
ENUM_DECLARATION NIST_QUALITY
{
   /// NFIQ value 5
   QUAL_POOR      =  20,
   /// NFIQ value 4
   QUAL_FAIR      =  40,
   /// NFIQ value 3
   QUAL_GOOD      =  60,
   /// NFIQ value 2
   QUAL_VGOOD     =  80,
   /// NFIQ value 1
   QUAL_EXCELLENT = 100
};


/** @brief Structure keeps information about image format
    @class IMAGE_FORMAT
*/
ENUM_DECLARATION IMAGE_FORMAT
{
    /// Undefined image format (the type will be detected automatically for decoding process).
    UNDEFINED = 0,
    /// WSQ image format with grayscale (8 bit) pixel format (used only for fingerprint images).
    WSQ       = 1,
    /// BMP image format.
    BMP       = 2,
    /// JPEG 2000 image format.
    JPEG2000  = 3,
    /// JPEG image format.
    JPEG      = 4,
    /// PNG image format.
    PNG       = 5,
    /// TIFF image format.
    TIFF      = 6,
    /// Proprietary image format used only for face images.
    FACE_PROP = 7,
    /// RAW image format with grayscale (8 bit) or BGR (24 bit) pixel format.
    RAW       = 8,
    /// Face NN image format used only for face Images.
    FACE_NN   = 9
};


/** @brief Structure keeps information about image pixel format
    @class PIXEL_FORMAT
*/
ENUM_DECLARATION PIXEL_FORMAT
{
    /// Grayscale pixel format with 8 bits per pixel.
    GRAYSCALE = 0,
    /// BGR pixel format with 24 bits per pixel.
    BGR       = 1
};


/** @brief FINGERS
    @class FINGERS
    Finger position enumeration
*/
ENUM_DECLARATION FINGERS
{
#if defined __cplusplus
   /// Unknown finger
   FINGPOS_UK = (int32_t)NIST_POS_CODE::POS_CODE_U_FINGER,
   /// Right thumb
   FINGPOS_RT = (int32_t)NIST_POS_CODE::POS_CODE_R_THUMB,
   /// Right index finger
   FINGPOS_RI = (int32_t)NIST_POS_CODE::POS_CODE_R_INDEX_F,
   /// Right middle finger
   FINGPOS_RM = (int32_t)NIST_POS_CODE::POS_CODE_R_MIDDLE_F,
   /// Right ring finger
   FINGPOS_RR = (int32_t)NIST_POS_CODE::POS_CODE_R_RING_F,
   /// Right little finger
   FINGPOS_RL = (int32_t)NIST_POS_CODE::POS_CODE_R_LITTLE_F,
   /// Left thumb
   FINGPOS_LT = (int32_t)NIST_POS_CODE::POS_CODE_L_THUMB,
   /// Left index finger
   FINGPOS_LI = (int32_t)NIST_POS_CODE::POS_CODE_L_INDEX_F,
   /// Left middle finger
   FINGPOS_LM = (int32_t)NIST_POS_CODE::POS_CODE_L_MIDDLE_F,
   /// Left ring finger
   FINGPOS_LR = (int32_t)NIST_POS_CODE::POS_CODE_L_RING_F,
   /// Left little finger
   FINGPOS_LL = (int32_t)NIST_POS_CODE::POS_CODE_L_LITTLE_F,

   /// Palm position codes
   /// Unknown palm
   POS_CODE_U_PALM = (int32_t)NIST_POS_CODE::POS_CODE_U_PALM,
   /// Right full palm
   POS_CODE_R_FULL_PALM = (int32_t)NIST_POS_CODE::POS_CODE_R_FULL_PALM,
   /// Right writer�s palm
   POS_CODE_R_WR_PALM = (int32_t)NIST_POS_CODE::POS_CODE_R_WR_PALM,
   /// Left full palm
   POS_CODE_L_FULL_PALM = (int32_t)NIST_POS_CODE::POS_CODE_L_FULL_PALM,
   /// Left writer�s palm
   POS_CODE_L_WR_PALM = (int32_t)NIST_POS_CODE::POS_CODE_L_WR_PALM,
   /// Right lower palm
   POS_CODE_R_LOWER_PALM = (int32_t)NIST_POS_CODE::POS_CODE_R_LOWER_PALM,
   /// Right upper palm
   POS_CODE_R_UPPER_PALM = (int32_t)NIST_POS_CODE::POS_CODE_R_UPPER_PALM,
   /// Left lower palm
   POS_CODE_L_LOWER_PALM = (int32_t)NIST_POS_CODE::POS_CODE_L_LOWER_PALM,
   /// Left upper palm
   POS_CODE_L_UPPER_PALM = (int32_t)NIST_POS_CODE::POS_CODE_L_UPPER_PALM,
   /// Right other
   POS_CODE_R_OTHER = (int32_t)NIST_POS_CODE::POS_CODE_R_OTHER,
   /// Left other
   POS_CODE_L_OTHER = (int32_t)NIST_POS_CODE::POS_CODE_L_OTHER,
   /// Right interdigital
   POS_CODE_R_INTERDIGITAL = (int32_t)NIST_POS_CODE::POS_CODE_R_INTERDIGITAL,
   /// Right thenar
   POS_CODE_R_THENAR = (int32_t)NIST_POS_CODE::POS_CODE_R_THENAR,
   /// Right hypothenar
   POS_CODE_R_HYPOTHENAR = (int32_t)NIST_POS_CODE::POS_CODE_R_HYPOTHENAR,
   /// Left interdigital
   POS_CODE_L_INTERDIGITAL = (int32_t)NIST_POS_CODE::POS_CODE_L_INTERDIGITAL,
   /// Left thenar
   POS_CODE_L_THENAR = (int32_t)NIST_POS_CODE::POS_CODE_L_THENAR,
   /// Left hypothenar
   POS_CODE_L_HYPOTHENAR = (int32_t)NIST_POS_CODE::POS_CODE_L_HYPOTHENAR
#else
    /// Unknown finger
    FINGPOS_UK = 0,
    /// Right thumb
    FINGPOS_RT = 1,
    /// Right index finger
    FINGPOS_RI = 2,
    /// Right middle finger
    FINGPOS_RM = 3,
    /// Right ring finger
    FINGPOS_RR = 4,
    /// Right little finger
    FINGPOS_RL = 5,
    /// Left thumb
    FINGPOS_LT = 6,
    /// Left index finger
    FINGPOS_LI = 7,
    /// Left middle finger
    FINGPOS_LM = 8,
    /// Left ring finger
    FINGPOS_LR = 9,
    /// Left little finger
    FINGPOS_LL = 10,

#endif // __cplusplus
};


/** @brief MinexTemplateType
    @class MinexTemplateType
    enumerates types of minex template
*/
ENUM_DECLARATION MinexTemplateType
{
   /// NIST INCITS 378 template
   NIST_TEMPLATE    = 0,
   /// ISO 19794-2 template
   ISO_TEMPLATE     = 1,
   /// TECH5 compact proprietary template, based on NIST INCITS 378 template (only 3 bytes for each minutiae are used)
   NIST_T5_TEMPLATE = 2,
};

/** @brief BIO_TYPE
    @class BIO_TYPE
*/
ENUM_DECLARATION BIO_TYPE
{
    /// Finger type.
    FINGER        = 0,
    /// Face type.
    FACE          = 1,
    /// Iris type.
    IRIS          = 2,
    /// Scars, Marks and Tattoo type.
    SMT           = 3,
    /// Palm type.
    PALM          = 4,
    /// Latent finger type.
    LATENT        = 5,
    /// Latent palm type.
    LATENT_PALM   = 6,
    /// Tenprint type.
    TENPRINT      = 7
};

/** @brief Matching_type
    @class Matching_type
*/
ENUM_DECLARATION Matching_type
{
   /// OmniMatch matching type (Face and/or Finger and/or Iris)
   OMNI = -1,

   /// Finger matching type
   FINGER_MT = 0,
   /// Face matching type
   FACE_MT = 1,
   /// Iris matching type
   IRIS_MT = 2,

   /// Scars, Marks and Tattoo matching type
   SMT_MT = 3,

   /// TP-TP (tenprint to tenprint) matching type
   TP_TP = 4,
   /// LT-TP (finger Latent to tenprint) matching type
   LT_TP = 5,
   /// TP-LT (tenprint to finger Latent) matching type
   TP_LT = 6,
   /// LT-LT (finger latent to finger Latent) matching type
   LT_LT = 7,

   /// PP-PP Palm record to palm record matching type
   PP_PP = 8,
   /// LP-PP (latent palm to Palm record) matching type
   LPP_PP = 9,
   /// PP-LP (Palm record to latent palm) matching type
   PP_LPP = 10,
   /// LP-LP (latent palm to latent palm) matching type
   LPP_LPP = 11
};


#ifdef __cplusplus
inline const char* get_modality_name(BIO_TYPE bio_type)
{
   switch(bio_type)
   {
   case BIO_TYPE::FINGER:
      return "finger";
      break;
   case BIO_TYPE::FACE:
      return "face";
      break;
   case BIO_TYPE::IRIS:
      return "iris";
      break;
   case BIO_TYPE::SMT:
      return "SMT";
      break;
   case BIO_TYPE::PALM:
      return "palm";
      break;
   case BIO_TYPE::LATENT:
      return "latent";
      break;
   case BIO_TYPE::LATENT_PALM:
      return "latent_palm";
      break;
   case BIO_TYPE::TENPRINT:
      return "tenprint";
      break;
   default:
      assert(false);
      return "";
      break;
   }
}
inline uint8_t get_num_objects(BIO_TYPE bio_type)
{
   switch (bio_type)
   {
   case BIO_TYPE::FINGER:
      return 10;
      break;
   case BIO_TYPE::FACE:
   case BIO_TYPE::SMT:
      return 1;
      break;
   case BIO_TYPE::IRIS:
   case BIO_TYPE::PALM:
      return 2;
      break;
   case BIO_TYPE::LATENT:
   case BIO_TYPE::TENPRINT:
      return 20;
      break;
   default:
      assert(false);
      return 0;
      break;
   }
}
#else
   inline const char* get_modality_name(enum BIO_TYPE bio_type)
   {
      switch(bio_type)
      {
      case FINGER:
         return "finger";
         break;
      case FACE:
         return "face";
         break;
      case IRIS:
         return "iris";
         break;
      case SMT:
         return "SMT";
         break;
      case PALM:
         return "palm";
         break;
      case LATENT:
         return "latent";
         break;
      case LATENT_PALM:
         return "latent_palm";
         break;
      case TENPRINT:
         return "tenprint";
         break;
      default:
         assert(false);
         return "";
         break;
      }
   }
   inline uint8_t get_num_objects(enum BIO_TYPE bio_type)
   {
      switch (bio_type)
      {
      case FINGER:
         return 10;
         break;
      case FACE:
      case SMT:
         return 1;
         break;
      case IRIS:
      case PALM:
         return 2;
         break;
      case LATENT:
      case TENPRINT:
         return 20;
         break;
      default:
         assert(false);
         return 0;
         break;
      }
   }

#endif
/// template type
ENUM_DECLARATION TEMPLATE_TYPE
{
    /// NN template.
    TEMPLATE_NN      = 0,
    /// Proprietary template.
    TEMPLATE_PROP    = 1,
    /// Combined (NN + proprietary) template.
    TEMPLATE_NN_PROP = 2
};

/** @brief Keeps information about the version of face NN algorithm used.
    @class FACE_NN_ALGORITHM
*/
ENUM_DECLARATION FACE_NN_ALGORITHM
{
    /// Detroit v. 1.0
    DET_FACE_100                   = 0,
    /// Detroit light v. 1.0
    DET_FACE_100_LIGHT             = 1,
    /// Detroit BTP v. 1.0
    DET_FACE_100_BTP               = 2,
    /// Detroit light BTP v. 1.0
    DET_FACE_100_LIGHT_BTP         = 3,
    /// Detroit light BTP compact v. 1.0
    DET_FACE_100_LIGHT_BTP_COMPACT = 4,
};

/** @brief Keeps information about the version of finger NN algorithm used.
    @class FINGER_NN_ALGORITHM
*/
ENUM_DECLARATION FINGER_NN_ALGORITHM
{
    /// Chicago v. 1.0
    CHI_FINGER_100                   = 0,
    /// Chicago light v. 1.0
    CHI_FINGER_100_LIGHT             = 1,
    /// Chicago BTP v. 1.0
    CHI_FINGER_100_BTP               = 2,
    /// Chicago light BTP v. 1.0
    CHI_FINGER_100_LIGHT_BTP         = 3,
    /// Chicago light BTP compact v. 1.0
    CHI_FINGER_100_LIGHT_BTP_COMPACT = 4,
    /// Chicago v. 2.0, it's compatible with Chicago v.1.0, have slightly less accuracy, but tolerate to the children fingers
    /// so, it should be used instead of Chicago v. 1.0 in  projects, where children with age <= 12 will be enrolled
    CHI_FINGER_200                   = 5,
};

/** @brief Keeps information about the version of iris NN algorithm used.
    @class IRIS_NN_ALGORITHM
*/
ENUM_DECLARATION IRIS_NN_ALGORITHM
{
    /// Chicago v. 1.0
    CHI_IRIS_100       = 0,
    /// Chicago light v. 1.0 (reserved for the future, not implemented yet) 
    CHI_IRIS_100_LIGHT = 1,
    /// Chicago BTP v. 1.0
    CHI_IRIS_100_BTP   = 2,
    /// Lisbon v. 1.0
    LIS_IRIS_100       = 3,
    /// Lisbon BTP v. 1.0
    LIS_IRIS_100_BTP   = 4,
    /// Paris v. 1.0
    PAR_IRIS_100       = 5,
    /// Indore v. 1.0
    IND_IRIS_100       = 6,
};

/** @brief Keeps information about the version of SMT NN algorithm used.
    @class SMT_NN_ALGORITHM
*/
ENUM_DECLARATION SMT_NN_ALGORITHM
{
    /// Chicago v. 1.0
    CHI_SMT_100 = 0
};

/** @brief Keeps information about the version of palm NN algorithm used.
    @class PALM_NN_ALGORITHM
*/
ENUM_DECLARATION PALM_NN_ALGORITHM
{
    /// Chicago v. 1.0
    CHI_PALM_100 = 0,
};

/** @brief Keeps information about the version of finger latent NN algorithm used.
    @class LATENT_NN_ALGORITHM
*/
ENUM_DECLARATION LATENT_NN_ALGORITHM
{
   /// Chicago v. 1.0
   CHI_LATENT_100 = 0,
};

/** @brief Keeps information about the version of palm latent NN algorithm used.
    @class LATENT_PALM_NN_ALGORITHM
*/
ENUM_DECLARATION LATENT_PALM_NN_ALGORITHM
{
   /// Chicago v. 1.0
   CHI_LATENT_PALM_100 = 0,
};

/** @brief Keeps information about the version of tenprint NN algorithm used.
    @class TENPRINT_NN_ALGORITHM
*/
ENUM_DECLARATION TENPRINT_NN_ALGORITHM
{
    /// Chicago v. 1.0
    CHI_TENPRINT_100 = 0,
};

/** @brief Keeps information about the version of NN algorithms used.
    @class NnAlgorithms
*/
struct NnAlgorithms
{
    enum FACE_NN_ALGORITHM          m_face_algorithm;
    enum FINGER_NN_ALGORITHM        m_finger_algorithm;
    enum IRIS_NN_ALGORITHM          m_iris_algorithm;
    
    enum SMT_NN_ALGORITHM           m_smt_algorithm;
    enum PALM_NN_ALGORITHM          m_palm_algorithm;
    enum LATENT_NN_ALGORITHM        m_latent_algorithm;
    enum LATENT_PALM_NN_ALGORITHM   m_latent_palm_algorithm;
    enum TENPRINT_NN_ALGORITHM      m_tenprint_algorithm;

#ifdef __cplusplus
    NnAlgorithms()
    {
       set_default();
    }

    void set_default()
    {
        m_face_algorithm        = FACE_NN_ALGORITHM       ::DET_FACE_100       ;
        m_finger_algorithm      = FINGER_NN_ALGORITHM     ::CHI_FINGER_100     ;
        m_iris_algorithm        = IRIS_NN_ALGORITHM       ::CHI_IRIS_100       ;
                                                                               
        m_smt_algorithm         = SMT_NN_ALGORITHM        ::CHI_SMT_100        ;
        m_palm_algorithm        = PALM_NN_ALGORITHM       ::CHI_PALM_100       ;
        m_latent_algorithm      = LATENT_NN_ALGORITHM     ::CHI_LATENT_100     ;
        m_latent_palm_algorithm = LATENT_PALM_NN_ALGORITHM::CHI_LATENT_PALM_100;
        m_tenprint_algorithm    = TENPRINT_NN_ALGORITHM   ::CHI_TENPRINT_100   ;
    }

    NnAlgorithms(const NnAlgorithms &algorithm)
    {
        *this = algorithm;
    }

    NnAlgorithms &operator= (const NnAlgorithms &algorithm)
    {
        memcpy(this, &algorithm, sizeof(algorithm));
        return *this;
    }
#endif
};

/** @brief Keeps information about the version of finger proprietary algorithm used.
    @class FINGER_PROP_ALGORITHM
*/
ENUM_DECLARATION FINGER_PROP_ALGORITHM
{
    /// Proprietary Diana template format
    FINGER_PROP_DIANA = 0,
    /// NIST INCITS 378 template format
    FINGER_PROP_NIST  = 1,
    /// ISO 19794-2 template format
    FINGER_PROP_ISO   = 2,
    /// TECH5 compact proprietary template format, based on NIST INCITS 378 template
    /// (only 3 bytes for each minutiae are used)
    FINGER_PROP_T5C   = 3
};

/** @brief Keeps information about the version of iris proprietary algorithm used.
    @class IRIS_PROP_ALGORITHM
*/
ENUM_DECLARATION IRIS_PROP_ALGORITHM
{
    /// Proprietary Diana algorithm
    IRIS_PROP_DIANA = 0,
};

/** @brief Keeps information about the version of palm proprietary algorithm used.
    @class PALM_PROP_ALGORITHM
*/
ENUM_DECLARATION PALM_PROP_ALGORITHM
{
   /// Proprietary Diana template format
   PALM_PROP_DIANA = 0,
};

/** @brief Keeps information about the version of latent proprietary algorithm used.
    @class LATENT_PROP_ALGORITHM
*/
ENUM_DECLARATION LATENT_PROP_ALGORITHM
{
   /// Proprietary Diana template format
   LATENT_PROP_DIANA = 0,
};

/** @brief Keeps information about the version of latent palm proprietary algorithm used.
    @class LATENT_PALM_PROP_ALGORITHM
*/
ENUM_DECLARATION LATENT_PALM_PROP_ALGORITHM
{
   /// Proprietary Diana template format
   LATENT_PALM_PROP_DIANA = 0,
};

/** @brief Keeps information about the version of tenprint proprietary algorithm used.
    @class TENPRINT_PROP_ALGORITHM
*/
ENUM_DECLARATION TENPRINT_PROP_ALGORITHM
{
    /// Proprietary Diana template format
    TENPRINT_PROP_DIANA = 0,
};

/** @brief Keeps information about the version of finger and iris proprietary algorithm used.
    @class PropAlgorithms
*/
struct PropAlgorithms
{
    enum FINGER_PROP_ALGORITHM        m_finger_algorithm;
    enum IRIS_PROP_ALGORITHM          m_iris_algorithm;

    enum PALM_PROP_ALGORITHM          m_palm_algorithm;
    enum LATENT_PROP_ALGORITHM        m_latent_algorithm;
    enum LATENT_PALM_PROP_ALGORITHM   m_latent_palm_algorithm;
    enum TENPRINT_PROP_ALGORITHM      m_tenprint_algorithm;

#ifdef __cplusplus
    PropAlgorithms()
    {
      set_default();
    }

    void set_default()
    {
        m_finger_algorithm      = FINGER_PROP_ALGORITHM     ::FINGER_PROP_DIANA     ;
        m_iris_algorithm        = IRIS_PROP_ALGORITHM       ::IRIS_PROP_DIANA       ;
        m_palm_algorithm        = PALM_PROP_ALGORITHM       ::PALM_PROP_DIANA       ;
        m_latent_algorithm      = LATENT_PROP_ALGORITHM     ::LATENT_PROP_DIANA     ;
        m_latent_palm_algorithm = LATENT_PALM_PROP_ALGORITHM::LATENT_PALM_PROP_DIANA;
        m_tenprint_algorithm    = TENPRINT_PROP_ALGORITHM   ::TENPRINT_PROP_DIANA   ;
    }

    PropAlgorithms(const PropAlgorithms &algorithm)
    {
        *this = algorithm;
    }

    PropAlgorithms &operator= (const PropAlgorithms &algorithm)
    {
        memcpy(this, &algorithm, sizeof(algorithm));
        return *this;
    }
#endif
};


/// maximum 500 dpi image width
const int32_t MAX_WIDTH  = (4 * 1024);
/// maximum 500 dpi image height
const int32_t MAX_HEIGHT = (4 * 1024);

/** @brief MINUTIAE_TYPE
    @class MINUTIAE_TYPE
    Minutiae types enumeration
*/
ENUM_DECLARATION MINUTIAE_TYPE
{
   /// bifurcation
   BIFURCATION = 0,
   /// ending
   ENDING      = 1
};


/** @brief RawImage
    @class RawImage
    structure keeps information about RAW image
*/
struct RawImage
{
   /// finger position
   enum FINGERS        m_finger;
   /// image width, pixels 
   uint32_t            m_width;
   /// image height, pixels
   uint32_t            m_height;
   /// image buffer
   uint8_t            *m_image;
   /// Image resolution (e.g. 500 , 1000)
   uint16_t            m_ppi;
   /// impression type
   enum NIST_IMP_TYPE  m_impression_type;

#ifdef __cplusplus
   RawImage()
   {
      clear();
   }
   void clear()
   {
      m_finger = FINGERS::FINGPOS_UK;
      m_width = 0;
      m_height = 0;
      m_image = nullptr;
      m_ppi = 0;
      m_impression_type = NIST_IMP_TYPE::IMP_TYPE_FP_LS_PL;
   }
#endif
};


/** @brief ImageData
    @class ImageData
   structure keeps information about image data
*/
struct ImageData
{
   /// Image format (@see IMAGE_FORMAT).
   enum IMAGE_FORMAT   m_format;
   /// Pointer to buffer with whole image file content.
   uint8_t            *m_image;
   /// Size of image.
   uint32_t            m_imageSize;
   /// Image resolution (e.g. 500 , 1000).
   uint16_t            m_ppi;
   /// Finger Position, 0...10 refer to ANSI/NIST standard (@see FINGERS).
   enum FINGERS        m_fingerPos;
   /// Impression type (@see NIST_IMP_TYPE).
   enum NIST_IMP_TYPE  m_impression_type;
   /// Age of a person
   uint8_t             m_personAge;
   /// Protection checking for wsq image format
   bool                m_checkProtection;

#ifdef __cplusplus
   ImageData()
   {
      clean();
   }

   ImageData& operator= (const ImageData& data)
   {
      memcpy(this, &data, sizeof(data));
      return *this;
   }

   ImageData(const ImageData& data)
   {
      *this = data;
   }

   void clean()
   {
      m_format          = IMAGE_FORMAT::UNDEFINED         ;
      m_image           = nullptr                         ;
      m_imageSize       = 0                               ;
      m_ppi             = 0                               ;
      m_fingerPos       = FINGERS::FINGPOS_UK             ;
      m_impression_type = NIST_IMP_TYPE::IMP_TYPE_FP_LS_PL;
      m_personAge       = 0;
      m_checkProtection = false;
   }
#endif
};

#pragma pack(push, _CORE_TIGHT)
/** @brief PersonalData
   @class PersonalData
*/
struct P_PACKED_1 PersonalData
{
   /// sex (of the Sex values)
   uint8_t        m_sex;
   /// year of birth 
   uint16_t       m_birthYear;
   /// less than 4 fingers was found in left   slap
   uint8_t        m_leftSlapError;
   /// less than 4 fingers was found in right  slap
   uint8_t        m_rightSlapError;
   /// less than 2 fingers was found in thumbs slap
   uint8_t        m_thumbsError;
   /// region number (or any other data that are used for binning) 0 - means not using this data for binning
   uint16_t       m_region;
   /// sizeof of this structure should be aligned to 16!!!
   uint8_t        m_reserved[8];
#ifdef __cplusplus
   /// creates PersonalData object
   PersonalData()
   {
      clear();
   }

   /// avoids personal data
   void clear()
   {
      memset(this, 0, sizeof(PersonalData));
   }

   /// copies the object
   PersonalData& operator=(const PersonalData& pd)
   {
      memcpy(this, &pd, sizeof(PersonalData));
      return *this;
   }

   /// creates a copy of object
   PersonalData(const PersonalData& pd)
   {
      *this = pd;
   }
#endif // __cplusplus
};
#pragma pack(pop)

#pragma pack(push, _CORE_PACKING)
/** @brief MatchResult
    @class MatchResult
    keeps information about matching results
*/
struct P_PACKED_8 MatchResult
{
   /// probe finger number 
   enum FINGERS fingerP;
   /// gallery finger number
   enum FINGERS fingerG;
   /// similarity score of compared fingerprints in a range 0...MAX_SCORE
   int32_t      similarity;
   /// number of found minutiae pairs
   uint32_t     numPairs;
   /// number of corresponded minutiae for probe and gallery template
   ///those were found while matching
   int16_t      minutiaeNum[MAX_MINUTIAE][2];
   /// information for combine fingerprint images:
   /// x coordinate of found minutiae center on probe fingerprint
   int32_t      xcP;
   /// y coordinate of found minutiae center on probe fingerprint
   int32_t      ycP;
   /// x coordinate of found minutiae center on gallery fingerprint 
   int32_t      xcG;
   /// y coordinate of found minutiae center on gallery fingerprint 
   int32_t      ycG;
   /// clockwise angle rotation of gallery fingerprint relatively probe fingerprint around found minutiae center
   int32_t      angle;

#ifdef __cplusplus
   MatchResult()
   {
      clear();
   }

   void clear()
   {
      memset(this, 0, sizeof(MatchResult));
   }
#endif
};
#pragma pack (pop)

/** @brief Feature
    @class Feature
    structure keeps information about position any features (minutiae and singularity)
    NOTE: all coordinates is applied to 500 DPI fingerprint image
*/
struct  Feature
{
   /// x position from top-left corner, pixels
   int32_t x;
   /// y position from top-left corner, pixels
   int32_t y;
   /// probability, %
   uint8_t prob;
   /// direction (clockwise from OX axis), degree (-180...180)
   int32_t angle;
   /// distance between ridges, pixels
   uint8_t density;

#ifdef __cplusplus
   Feature()
   {
      x       = 0;
      y       = 0;
      prob    = 0;
      angle   = 0;
      density = 0;
   }
#endif
};

/** @brief Minutiae
    @class Minutiae
    structure keeps information about minutiae
    NOTE: all coordinates is applied to 500 DPI fingerprint image
*/
#ifdef __cplusplus
struct Minutiae : public Feature
{
   /// type of minutiae defined in @ref MINUTIAE_TYPE
   enum MINUTIAE_TYPE type;

   Minutiae() : Feature()
   {
      type = MINUTIAE_TYPE::BIFURCATION;
   }
};
#else
struct Minutiae
{
   /// x position from top-left corner, pixels
   int32_t            x;
   /// y position from top-left corner, pixels
   int32_t            y;
   /// probability, %
   uint8_t            prob;
   /// direction (clockwise from OX axis), degree (-180...180)
   int32_t            angle;
   /// distance between ridges, pixels
   uint8_t            density;
   /// minutiae type
   enum MINUTIAE_TYPE type;
};
#endif

#ifdef __cplusplus
   } // extern "C"{
#endif

#if defined __cplusplus
} // namespace Tech5_core {
#endif

#endif // COMMON_H__
