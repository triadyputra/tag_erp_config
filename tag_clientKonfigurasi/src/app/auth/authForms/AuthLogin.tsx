'use client'

import React, { useEffect, useState } from 'react'
import {
  Box,
  Button,
  Stack,
  Typography,
  Alert,
  CircularProgress,
  IconButton,
  InputAdornment,
} from '@mui/material'
import { IconEye, IconEyeOff } from '@tabler/icons-react'
import { useRouter, useSearchParams } from 'next/navigation'

import { loginType } from '@/app/(DashboardLayout)/types/auth/auth'
import CustomTextField from '@/app/components/forms/theme-elements/CustomTextField'
import CustomFormLabel from '@/app/components/forms/theme-elements/CustomFormLabel'
import { login } from '@/services/auth.service'
import { setAuthToken } from '@/helpers/token.helper'

const AuthLogin = ({ title, subtitle, subtext }: loginType) => {
  const router = useRouter()
  const searchParams = useSearchParams()
  const next = searchParams.get('next') || '/'

  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  // 🔒 Redirect kalau sudah login
  useEffect(() => {
    const token = localStorage.getItem('access_token')
    if (token) router.replace('/')
  }, [router])

  const handleLogin = async () => {
    if (!username || !password) {
      setError('Username dan password wajib diisi')
      return
    }

    setLoading(true)
    setError('')

    try {
      const res = await login({ username, password })

      setAuthToken(
        res.token,
        res.refreshToken,
        res.expiresIn
      )

      window.location.replace(next)
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Terjadi kesalahan')
    } finally {
      setLoading(false)
    }
  }

  return (
    <>
      {title && (
        <Typography fontWeight={700} variant="h3" mb={1}>
          {title}
        </Typography>
      )}

      {subtext}

      <Box
        component="form"
        onSubmit={(e) => {
          e.preventDefault()
          handleLogin()
        }}
      >
        <Stack spacing={0} mt={2}>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <Box>
            <CustomFormLabel sx={{ mb: 0.5 }}>Username</CustomFormLabel>
            <CustomTextField
              fullWidth
              value={username}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                setUsername(e.target.value)
              }
              sx={{
                '& .MuiInputBase-root': {
                  height: 48,
                },
                mb: 0,
              }}
            />
          </Box>

          <Box mb={2}>
            <CustomFormLabel sx={{ mb: 0.5 }}>Password</CustomFormLabel>
            <CustomTextField
              type={showPassword ? 'text' : 'password'}
              fullWidth
              value={password}
              onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                setPassword(e.target.value)
              }
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={() => setShowPassword((prev) => !prev)}
                      edge="end"
                      tabIndex={-1}
                    >
                      {showPassword ? (
                        <IconEyeOff size={20} />
                      ) : (
                        <IconEye size={20} />
                      )}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
              sx={{
                '& .MuiInputBase-root': {
                  height: 48,
                },
                mb: 0,
              }}
            />
          </Box>

          <Button
            type="submit"
            variant="contained"
            size="large"
            fullWidth
            disabled={loading}
            sx={{
              height: 48,
              fontWeight: 600,
              mt: 1,
            }}
          >
            {loading ? <CircularProgress size={22} color="inherit" /> : 'Sign In'}
          </Button>
        </Stack>
      </Box>

      {subtitle}
    </>
  )
}

export default AuthLogin