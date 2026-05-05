import Link from 'next/link'
import Box from '@mui/material/Box'
import Grid from '@mui/material/Grid'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import PageContainer from '@/app/components/container/PageContainer'
import Logo from '@/app/(DashboardLayout)/layout/shared/logo/Logo'
import AuthLogin from '../../authForms/AuthLogin'
import Image from 'next/image'

export default function Login() {
  return (
    <PageContainer title="Login Page" description="CPC Login">
      <Grid container spacing={0} sx={{ height: '100vh' }}>
        
        {/* ================= LEFT SIDE ================= */}
        <Grid
          sx={{
            position: 'relative',
            '&:before': {
              content: '""',
              background: 'radial-gradient(#d2f1df, #d3d7fa, #bad8f4)',
              backgroundSize: '400% 400%',
              animation: 'gradient 15s ease infinite',
              position: 'absolute',
              inset: 0,
              opacity: 0.3,
            },
          }}
          size={{ xs: 12, lg: 7, xl: 8 }}
        >
          <Box position="relative" height="100%">
            <Box px={3} py={2}>
              <Logo />
            </Box>

            <Box
              display={{ xs: 'none', lg: 'flex' }}
              alignItems="center"
              justifyContent="center"
              height="calc(100vh - 80px)"
            >
              <Image
                src="/images/logos/tag_icon.png"
                alt="Login Illustration"
                width={400}
                height={400}
                style={{
                  width: '100%',
                  maxWidth: 420,
                  maxHeight: 420,
                }}
              />
            </Box>
          </Box>
        </Grid>

        {/* ================= RIGHT SIDE ================= */}
        <Grid
          display="flex"
          justifyContent="center"
          alignItems="center"
          size={{ xs: 12, lg: 5, xl: 4 }}
        >
          {/* 🔥 INI KUNCI PROPORSI */}
          <Box
            sx={{
              width: '100%',
              maxWidth: 420,   // ⬅️ FIX: bikin form kelihatan seimbang
              px: 4,
              py: 6,
            }}
          >
            <AuthLogin
              title="Welcome to TAG"
              subtext={
                <Typography variant="subtitle1" color="textSecondary" mb={2}>
                  System Configuration & Access Control
                </Typography>
              }
            />
          </Box>
        </Grid>

      </Grid>
    </PageContainer>
  )
}
