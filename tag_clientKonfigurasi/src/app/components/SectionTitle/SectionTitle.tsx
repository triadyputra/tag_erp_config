'use client'

import React from 'react'
import { Box, Typography } from '@mui/material'

interface Props {
  title: string
  subtitle?: string
}

const SectionTitle: React.FC<Props> = ({ title, subtitle }) => {
  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',

        mb: 2,          // 🔥 tambah jarak bawah (dari 1.2 → 2)
        mt: 0.5,        // 🔥 sedikit jarak atas biar lega

        pb: 0.5,
        borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
      }}
    >
      {/* garis kiri */}
      <Box
        sx={{
          width: 3,
          height: subtitle ? 32 : 20,
          borderRadius: 2,
          backgroundColor: (theme) => theme.palette.primary.main,
          mr: 1,
        }}
      />

      {/* text */}
      <Box>
        <Typography
          fontSize={13}
          fontWeight={700}
          lineHeight={1.2}
          sx={{
            color: (theme) => theme.palette.primary.main, // 🔥 warna judul
          }}
        >
          {title}
        </Typography>

        {subtitle && (
          <Typography
            fontSize={11}
            sx={{
              color: 'text.secondary',
              mt: 0.3,
            }}
          >
            {subtitle}
          </Typography>
        )}
      </Box>
    </Box>
  )
}

export default SectionTitle